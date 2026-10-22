export default {
  async fetch(request, env, ctx) {
    try {
      const url = new URL(request.url);

      // =============================
      // 1. Health check (GET /)
      // =============================
      if (request.method === "GET" && url.pathname === "/") {
        return json({
          status: "ok",
          service: "thaix-slack-edge",
          timestamp: new Date().toISOString()
        });
      }

      // =============================
      // 2. Slack endpoint (POST /slack)
      // =============================
      if (request.method === "POST" && url.pathname === "/slack") {
        return await handleSlackRequest(request, env, ctx);
      }

      // =============================
      // 3. Telegram webhook (POST /telegram)
      // =============================
      if (request.method === "POST" && url.pathname === "/telegram") {
        return await handleTelegramRequest(request, env, ctx);
      }

      // =============================
      // 4. Generic Proxy (POST /api/proxy)
      // =============================
      if (request.method === "POST" && url.pathname === "/api/proxy") {
        const {
          targetUrl,
          method = "GET",
          headers = {},
          queryParameters = {},
          body
        } = await request.json();

        if (!targetUrl) {
          return json({ error: "Target URL is required" }, 400);
        }

        const target = new URL(targetUrl);
        const normalizedMethod = String(method || "GET").toUpperCase();

        const safeHeaders = headers && typeof headers === "object" ? headers : {};
        const safeQueryParameters =
          queryParameters && typeof queryParameters === "object" ? queryParameters : {};

        // Append query params
        for (const [k, v] of Object.entries(safeQueryParameters)) {
          if (v !== undefined && v !== null) {
            target.searchParams.append(String(k), String(v));
          }
        }

        const fetchOptions = {
          method: normalizedMethod,
          headers: new Headers(safeHeaders)
        };

        // Attach body if applicable
        if (["POST", "PUT", "PATCH", "DELETE"].includes(normalizedMethod) && body !== undefined && body !== null) {
          if (typeof body === "string") {
            fetchOptions.body = body;
          } else {
            if (!fetchOptions.headers.has("Content-Type")) {
              fetchOptions.headers.set("Content-Type", "application/json");
            }
            fetchOptions.body = JSON.stringify(body);
          }
        }

        const response = await fetch(target.toString(), fetchOptions);

        return new Response(response.body, {
          status: response.status,
          headers: response.headers
        });
      }

      // =============================
      // 5. Fallback
      // =============================
      return json({ error: "Not found" }, 404);

    } catch (err) {
      return json({ error: "Internal server error" }, 500);
    }
  }
};

// =============================
// Module-level M2M token cache
// (per Worker instance; refreshed when close to expiry)
// =============================
let _cachedToken = null;
let _tokenExpiresAt = 0;

async function fetchApiToken(env) {
  const clientId = String(env.BACKEND_CLIENT_ID || "").trim();
  const clientSecret = String(env.BACKEND_CLIENT_SECRET || "").trim();

  if (!clientId || !clientSecret) {
    return null;
  }

  const backendBaseUrl = String(env.BACKEND_URL).replace(/\/+$/, "");

  let response;
  try {
    response = await fetch(`${backendBaseUrl}/api/token/client-credentials`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        clientId,
        clientSecret,
        scope: "BotCommand.Execute"
      })
    });
  } catch {
    return null;
  }

  if (!response.ok) {
    return null;
  }

  let body;
  try {
    body = await response.json();
  } catch {
    return null;
  }

  const token = body?.data?.accessToken;
  const expiresIn = body?.data?.expiresIn ?? 3600;

  if (!token) {
    return null;
  }

  _cachedToken = token;
  // Refresh 60 seconds before expiry to avoid clock skew
  _tokenExpiresAt = Date.now() + (expiresIn - 60) * 1000;

  return token;
}

async function getApiToken(env) {
  if (_cachedToken && Date.now() < _tokenExpiresAt) {
    return _cachedToken;
  }
  return await fetchApiToken(env);
}

// =============================
// Helper
// =============================
function json(data, status = 200) {
  return new Response(JSON.stringify(data, null, 2), {
    status,
    headers: {
      "Content-Type": "application/json"
    }
  });
}

async function handleSlackRequest(request, env, ctx) {
  if (!env.BACKEND_URL) {
    return json({ error: "BACKEND_URL not configured" }, 500);
  }

  if (!env.SLACK_SIGNING_SECRET) {
    return json({ error: "SLACK_SIGNING_SECRET not configured" }, 500);
  }

  const rawBody = await request.text();
  const contentType = (request.headers.get("content-type") || "").toLowerCase();

  const signatureValid = await verifySlackSignature(request, rawBody, env.SLACK_SIGNING_SECRET);
  if (!signatureValid.ok) {
    return json({ error: signatureValid.reason }, 401);
  }

  if (contentType.includes("application/json")) {
    return await handleSlackEventRequest(rawBody, env, ctx);
  }

  const form = new URLSearchParams(rawBody);

  // Slack certificate checks may send ssl_check=1.
  if (form.get("ssl_check") === "1") {
    return new Response("", { status: 200 });
  }

  const command = (form.get("command") || "").trim();
  const text = (form.get("text") || "").trim();
  const userId = (form.get("user_id") || "").trim();
  const channelId = (form.get("channel_id") || "").trim();
  const responseUrl = (form.get("response_url") || "").trim();

  if (!userId || !channelId) {
    return slackResponse("Invalid Slack payload: user_id or channel_id is missing.", "ephemeral", 400);
  }

  const backendRequest = {
    command,
    text,
    externalUserId: userId,
    externalChannelId: channelId
  };

  // Pass responseUrl so backend can send the result directly.
  backendRequest.responseUrl = responseUrl || "";

  // Fire-and-forget: backend sends response directly via Slack API.
  ctx.waitUntil(executeBackendCommand(backendRequest, env));

  // Always ACK immediately for Slack timeout safety.
  return slackResponse("Command received. Processing...", "ephemeral", 200);
}

async function handleSlackEventRequest(rawBody, env, ctx) {
  let payload;
  try {
    payload = JSON.parse(rawBody);
  } catch {
    return json({ error: "Invalid Slack JSON payload" }, 400);
  }

  if (payload && payload.type === "url_verification" && typeof payload.challenge === "string") {
    return new Response(payload.challenge, {
      status: 200,
      headers: {
        "Content-Type": "text/plain"
      }
    });
  }

  if (!payload || payload.type !== "event_callback") {
    return new Response("", { status: 200 });
  }

  const event = payload.event && typeof payload.event === "object" ? payload.event : null;
  if (!event || event.type !== "app_mention") {
    return new Response("", { status: 200 });
  }

  if (event.bot_id || event.subtype === "bot_message") {
    return new Response("", { status: 200 });
  }

  const userId = String(event.user || "").trim();
  const channelId = String(event.channel || "").trim();
  const text = String(event.text || "").trim();
  const threadTs = String(event.thread_ts || event.ts || "").trim();

  if (!userId || !channelId) {
    return new Response("", { status: 200 });
  }

  const cleanedText = normalizeMentionText(text);
  if (!cleanedText) {
    ctx.waitUntil(postSlackThreadMessage(
      {
        channel: channelId,
        threadTs,
        text: `<@${userId}> Please provide a command after mentioning me.`
      },
      env
    ));
    return new Response("", { status: 200 });
  }

  const backendRequest = {
    command: "",
    text: cleanedText,
    externalUserId: userId,
    externalChannelId: channelId
  };

  // Pass threadTs so backend can reply in the thread.
  backendRequest.threadTs = threadTs;

  // Fire-and-forget: backend sends response directly via Slack API.
  ctx.waitUntil(executeBackendCommand(backendRequest, env));

  return new Response("", { status: 200 });
}

function normalizeMentionText(text) {
  if (typeof text !== "string") {
    return "";
  }

  return text
    .replace(/<@[^>]+>/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

async function postSlackThreadMessage(message, env) {
  const botToken = (env.SLACK_BOT_TOKEN || "").trim();
  if (!botToken) {
    return;
  }

  const payload = {
    channel: message.channel,
    text: message.text,
    thread_ts: message.threadTs,
    unfurl_links: false,
    unfurl_media: false
  };

  if (Array.isArray(message.blocks) && message.blocks.length > 0) {
    payload.blocks = message.blocks;
  }

  await fetch("https://slack.com/api/chat.postMessage", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${botToken}`
    },
    body: JSON.stringify(payload)
  });
}

async function executeBackendCommand(payload, env) {
  const backendBaseUrl = String(env.BACKEND_URL).replace(/\/+$/, "");
  const headers = {
    "Content-Type": "application/json"
  };

  const token = await getApiToken(env);
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${backendBaseUrl}/api/slack/commands/execute`, {
    method: "POST",
    headers,
    body: JSON.stringify(payload)
  });

  let body = null;
  try {
    body = await response.json();
  } catch {
    body = null;
  }

  return {
    ok: response.ok,
    status: response.status,
    body
  };
}

function slackResponse(text, responseType = "ephemeral", status = 200) {
  return new Response(JSON.stringify({
    response_type: responseType,
    text
  }), {
    status,
    headers: {
      "Content-Type": "application/json"
    }
  });
}

async function verifySlackSignature(request, rawBody, signingSecret) {
  const timestamp = request.headers.get("x-slack-request-timestamp") || "";
  const signature = request.headers.get("x-slack-signature") || "";

  if (!timestamp || !signature) {
    return { ok: false, reason: "Missing Slack signature headers." };
  }

  const requestTimeSeconds = Number.parseInt(timestamp, 10);
  if (!Number.isFinite(requestTimeSeconds)) {
    return { ok: false, reason: "Invalid Slack request timestamp." };
  }

  const nowSeconds = Math.floor(Date.now() / 1000);
  if (Math.abs(nowSeconds - requestTimeSeconds) > 60 * 5) {
    return { ok: false, reason: "Slack request timestamp is out of range." };
  }

  const basestring = `v0:${timestamp}:${rawBody}`;
  const computed = await computeSlackSignature(signingSecret, basestring);

  if (!safeEquals(computed, signature)) {
    return { ok: false, reason: "Invalid Slack signature." };
  }

  return { ok: true, reason: "" };
}

async function computeSlackSignature(signingSecret, basestring) {
  const keyData = new TextEncoder().encode(signingSecret);
  const messageData = new TextEncoder().encode(basestring);

  const key = await crypto.subtle.importKey(
    "raw",
    keyData,
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign"]
  );

  const signature = await crypto.subtle.sign("HMAC", key, messageData);
  const hashHex = toHex(signature);
  return `v0=${hashHex}`;
}

function toHex(buffer) {
  return Array.from(new Uint8Array(buffer))
    .map((b) => b.toString(16).padStart(2, "0"))
    .join("");
}

function safeEquals(a, b) {
  if (typeof a !== "string" || typeof b !== "string") {
    return false;
  }

  if (a.length !== b.length) {
    return false;
  }

  let out = 0;
  for (let i = 0; i < a.length; i++) {
    out |= a.charCodeAt(i) ^ b.charCodeAt(i);
  }

  return out === 0;
}

// =============================
// Telegram webhook handling
// =============================

async function handleTelegramRequest(request, env, ctx) {
  if (!env.TELEGRAM_BOT_TOKEN) {
    return json({ error: "TELEGRAM_BOT_TOKEN not configured" }, 500);
  }

  const rawBody = await request.text();
  let update;
  try {
    update = JSON.parse(rawBody);
  } catch {
    return json({ error: "Invalid Telegram JSON payload" }, 400);
  }

  if (env.TELEGRAM_WEBHOOK_SECRET) {
    const secret = request.headers.get("X-Telegram-Bot-Api-Secret-Token") || "";
    if (!safeEquals(secret, String(env.TELEGRAM_WEBHOOK_SECRET))) {
      return json({ error: "Unauthorized" }, 401);
    }
  }

  const message = update?.message;
  if (!message || typeof message !== "object") {
    return new Response("", { status: 200 });
  }

  if (message.from?.is_bot) {
    return new Response("", { status: 200 });
  }

  const chatId = String(message.chat?.id || "").trim();
  const userId = String(message.from?.id || "").trim();
  const chatType = String(message.chat?.type || "");
  const rawText = String(message.text || message.caption || "");
  const text = rawText.trim();
  const botUsername = String(env.TELEGRAM_BOT_USERNAME || "").trim().replace(/^@/, "");

  if (!chatId || !userId || !text) {
    return new Response("", { status: 200 });
  }

  const commandMatch = /^\/([A-Za-z0-9_]+)(?:@([A-Za-z0-9_]+))?(?:\s+([\s\S]*))?$/.exec(text);
  let commandText;

  if (commandMatch) {
    const targetUsername = commandMatch[2];
    if (targetUsername && (!botUsername || targetUsername.toLowerCase() !== botUsername.toLowerCase())) {
      return new Response("", { status: 200 });
    }

    commandText = "/" + commandMatch[1] + (commandMatch[3] ? " " + commandMatch[3].trim() : "");
  } else if (chatType === "private") {
    const mentionMatch = /^@\w+\s*([\s\S]*)$/.exec(text);
    commandText = mentionMatch ? mentionMatch[1].trim() : text;
  } else if (chatType === "group" || chatType === "supergroup") {
    if (!botUsername) {
      return new Response("", { status: 200 });
    }

    const isReplyToThisBot = message.reply_to_message?.from?.is_bot === true
      && String(message.reply_to_message.from.username || "").toLowerCase() === botUsername.toLowerCase();
    const entities = typeof message.text === "string" ? message.entities : message.caption_entities;
    const botMention = findTelegramBotMention(rawText, entities, botUsername);

    if (!isReplyToThisBot && !botMention) {
      return new Response("", { status: 200 });
    }

    commandText = botMention
      ? removeTelegramEntity(text, botMention)
      : text;
  } else {
    return new Response("", { status: 200 });
  }

  if (!commandText) {
    return new Response("", { status: 200 });
  }

  // Fire-and-forget: backend sends response directly to the chat via Telegram Bot API.
  ctx.waitUntil(executeTelegramCommand(
    { text: commandText, externalUserId: userId, externalChatId: chatId },
    env
  ));

  return new Response("", { status: 200 });
}

function findTelegramBotMention(text, entities, botUsername) {
  if (!Array.isArray(entities)) {
    return null;
  }

  const expectedMention = "@" + botUsername.toLowerCase();
  return entities.find(entity => {
    if (!Number.isInteger(entity.offset) || !Number.isInteger(entity.length)) {
      return false;
    }

    if (entity.type === "mention") {
      return text.slice(entity.offset, entity.offset + entity.length).toLowerCase() === expectedMention;
    }

    return entity.type === "text_mention"
      && String(entity.user?.username || "").toLowerCase() === botUsername.toLowerCase();
  }) || null;
}

function removeTelegramEntity(text, entity) {
  return (text.slice(0, entity.offset) + " " + text.slice(entity.offset + entity.length))
    .replace(/\s+/g, " ")
    .trim();
}

async function executeTelegramCommand(payload, env) {
  const backendBaseUrl = String(env.BACKEND_URL).replace(/\/+$/, "");
  const headers = { "Content-Type": "application/json" };

  const token = await getApiToken(env);
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${backendBaseUrl}/api/telegram/webhook`, {
    method: "POST",
    headers,
    body: JSON.stringify(payload)
  });

  let body = null;
  try {
    body = await response.json();
  } catch {
    body = null;
  }

  return { ok: response.ok, status: response.status, body };
}

async function fetchTelegram(method, env, params) {
  const botToken = String(env.TELEGRAM_BOT_TOKEN || "").trim();
  if (!botToken) {
    return;
  }

  try {
    await fetch(`https://api.telegram.org/bot${botToken}/${method}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(params)
    });
  } catch {
    // Best-effort; Telegram will retry if we don't ACK.
  }
}
