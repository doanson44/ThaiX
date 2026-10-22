# Slack conversation context setup

The Worker forwards ordinary human Slack messages to the ThaiX backend for history storage. It only asks the backend to generate a response for `app_mention` events; slash commands continue through the existing slash-command flow.

## Slack app configuration

In **Event Subscriptions**, subscribe to these bot events:

- `app_mention`
- `message.channels` for public channels
- `message.groups` for private channels

To retain conversations from direct messages or multi-person DMs too, also subscribe to `message.im` and `message.mpim`.

Ensure the app has the corresponding history scopes for subscribed event types, plus its existing bot scopes for posting replies and slash commands. Reinstall the Slack app after changing OAuth scopes.

## Storage and behavior

- Channel messages are stored under `SLACK_CHANNEL_<channelId>`.
- Thread replies are stored under `SLACK_THREAD_<channelId>_<threadTs>`.
- The most recent five messages are supplied to conversational fallback prompts.
- Each history retains at most 50 messages, each capped at 2,000 characters, with a rolling 30-day expiration.
- Slack retries are deduplicated by message timestamp. History failures should not block bot responses.

After changing `cloudflare/_worker.js`, deploy the Cloudflare Worker again.
