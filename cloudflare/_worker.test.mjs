import assert from "node:assert/strict";
import test from "node:test";
import worker from "./_worker.js";

async function invokeTelegram(update, overrides = {}) {
  const originalFetch = globalThis.fetch;
  const calls = [];
  const pending = [];

  globalThis.fetch = async (input, init = {}) => {
    calls.push({
      url: String(input),
      body: init.body ? JSON.parse(init.body) : null
    });
    return new Response("{}", {
      status: 200,
      headers: { "Content-Type": "application/json" }
    });
  };

  try {
    const response = await worker.fetch(
      new Request("https://worker.example/telegram", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Telegram-Bot-Api-Secret-Token": "test-secret"
        },
        body: JSON.stringify(update)
      }),
      {
        TELEGRAM_BOT_TOKEN: "test-token",
        TELEGRAM_BOT_USERNAME: "thaix_bot",
        TELEGRAM_WEBHOOK_SECRET: "test-secret",
        BACKEND_URL: "https://backend.example",
        ...overrides
      },
      {
        waitUntil(promise) {
          pending.push(promise);
        }
      }
    );

    await Promise.all(pending);
    return { response, calls };
  } finally {
    globalThis.fetch = originalFetch;
  }
}

function telegramUpdate(message) {
  return { update_id: 1, message };
}

function groupMessage(text, extra = {}) {
  return {
    message_id: 10,
    chat: { id: -100123, type: "supergroup" },
    from: { id: 123, is_bot: false },
    text,
    ...extra
  };
}

test("forwards unaddressed plain text in a group for conversation history", async () => {
  const { response, calls } = await invokeTelegram(
    telegramUpdate(groupMessage("hello everyone"))
  );

  assert.equal(response.status, 200);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].body.text, "hello everyone");
  assert.equal(calls[0].body.chatType, "supergroup");
  assert.equal(calls[0].body.isBotMentioned, false);
});

test("forwards group slash commands", async () => {
  const { calls } = await invokeTelegram(
    telegramUpdate(groupMessage("/pc BTCUSDT"))
  );

  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, "https://backend.example/api/telegram/webhook");
  assert.equal(calls[0].body.text, "/pc BTCUSDT");
});

test("ignores slash commands addressed to another bot", async () => {
  const { calls } = await invokeTelegram(
    telegramUpdate(groupMessage("/help@other_bot"))
  );

  assert.equal(calls.length, 0);
});

test("forwards group messages that mention this bot", async () => {
  const text = "@ThaiX_Bot get btc price";
  const { calls } = await invokeTelegram(
    telegramUpdate(groupMessage(text, {
      entities: [{ type: "mention", offset: 0, length: 10 }]
    }))
  );

  assert.equal(calls.length, 1);
  assert.equal(calls[0].body.text, "get btc price");
  assert.equal(calls[0].body.isBotMentioned, true);
});

test("forwards group replies to this bot", async () => {
  const { calls } = await invokeTelegram(
    telegramUpdate(groupMessage("get btc price", {
      reply_to_message: {
        message_id: 9,
        from: { id: 999, is_bot: true, username: "thaix_bot" },
        chat: { id: -100123, type: "supergroup" },
        text: "What can I help with?"
      }
    }))
  );

  assert.equal(calls.length, 1);
  assert.equal(calls[0].body.text, "get btc price");
  assert.equal(calls[0].body.isReplyToBot, true);
});

test("preserves free text in private chats", async () => {
  const { calls } = await invokeTelegram(
    telegramUpdate({
      message_id: 11,
      chat: { id: 123, type: "private" },
      from: { id: 123, is_bot: false },
      text: "hello ThaiX"
    })
  );

  assert.equal(calls.length, 1);
  assert.equal(calls[0].body.text, "hello ThaiX");
});

test("forwards group text that mentions another bot for history only", async () => {
  const text = "@other_bot hello";
  const { calls } = await invokeTelegram(
    telegramUpdate(groupMessage(text, {
      entities: [{ type: "mention", offset: 0, length: 10 }]
    }))
  );

  assert.equal(calls.length, 1);
  assert.equal(calls[0].body.text, text);
  assert.equal(calls[0].body.isBotMentioned, false);
});


async function invokeSlackEvent(event, overrides = {}) {
  const originalFetch = globalThis.fetch;
  const calls = [];
  const pending = [];
  const rawBody = JSON.stringify({ type: "event_callback", event_id: "Ev123", event });
  const timestamp = String(Math.floor(Date.now() / 1000));
  const key = await crypto.subtle.importKey(
    "raw", new TextEncoder().encode("test-signing-secret"),
    { name: "HMAC", hash: "SHA-256" }, false, ["sign"]
  );
  const signatureBytes = await crypto.subtle.sign("HMAC", key, new TextEncoder().encode(`v0:${timestamp}:${rawBody}`));
  const signature = "v0=" + Array.from(new Uint8Array(signatureBytes))
    .map(value => value.toString(16).padStart(2, "0")).join("");

  globalThis.fetch = async (input, init = {}) => {
    calls.push({ url: String(input), body: init.body ? JSON.parse(init.body) : null });
    return new Response("{}", { status: 200, headers: { "Content-Type": "application/json" } });
  };

  try {
    const response = await worker.fetch(
      new Request("https://worker.example/slack", {
        method: "POST",
        headers: { "Content-Type": "application/json", "X-Slack-Request-Timestamp": timestamp, "X-Slack-Signature": signature },
        body: rawBody
      }),
      { SLACK_SIGNING_SECRET: "test-signing-secret", BACKEND_URL: "https://backend.example", ...overrides },
      { waitUntil(promise) { pending.push(promise); } }
    );
    await Promise.all(pending);
    return { response, calls };
  } finally {
    globalThis.fetch = originalFetch;
  }
}

test("forwards regular Slack messages for conversation history without triggering a reply", async () => {
  const { response, calls } = await invokeSlackEvent({
    type: "message", user: "U123", channel: "C123", ts: "1720000000.000001",
    text: "hello team", user_profile: { display_name: "Alice" }
  });
  assert.equal(response.status, 200);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].url, "https://backend.example/api/slack/commands/execute");
  assert.equal(calls[0].body.text, "hello team");
  assert.equal(calls[0].body.isConversationEvent, true);
  assert.equal(calls[0].body.isMention, false);
  assert.equal(calls[0].body.messageTs, "1720000000.000001");
  assert.equal(calls[0].body.senderName, "Alice");
});

test("forwards Slack app mentions with cleaned text and thread metadata", async () => {
  const { response, calls } = await invokeSlackEvent({
    type: "app_mention", user: "U123", channel: "C123", ts: "1720000000.000002",
    thread_ts: "1720000000.000000", text: "<@U999> hello ThaiX"
  });
  assert.equal(response.status, 200);
  assert.equal(calls.length, 1);
  assert.equal(calls[0].body.text, "hello ThaiX");
  assert.equal(calls[0].body.isConversationEvent, true);
  assert.equal(calls[0].body.isMention, true);
  assert.equal(calls[0].body.threadTs, "1720000000.000000");
  assert.equal(calls[0].body.conversationThreadTs, "1720000000.000000");
});

test("ignores Slack bot messages", async () => {
  const { calls } = await invokeSlackEvent({
    type: "message", bot_id: "B999", user: "U999", channel: "C123",
    ts: "1720000000.000003", text: "bot output"
  });
  assert.equal(calls.length, 0);
});
