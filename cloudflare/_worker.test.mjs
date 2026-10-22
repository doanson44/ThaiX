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

test("ignores unaddressed plain text in a group", async () => {
  const { response, calls } = await invokeTelegram(
    telegramUpdate(groupMessage("hello everyone"))
  );

  assert.equal(response.status, 200);
  assert.equal(calls.length, 0);
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

test("ignores group text that mentions another bot", async () => {
  const text = "@other_bot hello";
  const { calls } = await invokeTelegram(
    telegramUpdate(groupMessage(text, {
      entities: [{ type: "mention", offset: 0, length: 10 }]
    }))
  );

  assert.equal(calls.length, 0);
});
