# Slack Setup for ThaiX Cloudflare Worker

This guide configures Slack to send slash commands and app mention events to the ThaiX Cloudflare Worker.

## Slash Command Table (English)

| Command | Request URL | Short Description | Usage Hint |
| --- | --- | --- | --- |
| `/help` | `https://YOUR-WORKER.workers.dev/slack` | Show available commands | `[command]` |
| `/pc` | `https://YOUR-WORKER.workers.dev/slack` | Get crypto price | `<symbol>` |
| `/p` | `https://YOUR-WORKER.workers.dev/slack` | Get price for any symbol, auto-detects crypto or VnStock | `<symbol>` |
| `/ps` | `https://YOUR-WORKER.workers.dev/slack` | Get stock price | `<symbol>` |
| `/pci` | `https://YOUR-WORKER.workers.dev/slack` | Get crypto technical analysis | `<symbol> [timeframe]` |
| `/psi` | `https://YOUR-WORKER.workers.dev/slack` | Get stock technical analysis | `<symbol> [timeframe]` |
| `/pf` | `https://YOUR-WORKER.workers.dev/slack` | Show portfolio summary | `` |
| `/alert` | `https://YOUR-WORKER.workers.dev/slack` | Manage price alerts | `list | <symbol> >|< <price> [stock|crypto] [--repeat]` |
| `/note` | `https://YOUR-WORKER.workers.dev/slack` | Create a note or list notes | `<content> | list` |
| `/tcbs` | `https://YOUR-WORKER.workers.dev/slack` | Show TCBS Top 10 holdings | `` |

## Bảng Lệnh Slash Command (Tiếng Việt)

| Command | Request URL | Short Description | Usage Hint |
| --- | --- | --- | --- |
| `/help` | `https://YOUR-WORKER.workers.dev/slack` | Hiển thị danh sách lệnh | `[command]` |
| `/pc` | `https://YOUR-WORKER.workers.dev/slack` | Lấy giá crypto | `<symbol>` |
| `/p` | `https://YOUR-WORKER.workers.dev/slack` | Lấy giá cho mọi mã, tự động nhận diện crypto hoặc cổ phiếu VN | `<symbol>` |
| `/ps` | `https://YOUR-WORKER.workers.dev/slack` | Lấy giá cổ phiếu | `<symbol>` |
| `/pci` | `https://YOUR-WORKER.workers.dev/slack` | Phân tích kỹ thuật crypto | `<symbol> [timeframe]` |
| `/psi` | `https://YOUR-WORKER.workers.dev/slack` | Phân tích kỹ thuật cổ phiếu | `<symbol> [timeframe]` |
| `/pf` | `https://YOUR-WORKER.workers.dev/slack` | Xem tóm tắt portfolio | `` |
| `/alert` | `https://YOUR-WORKER.workers.dev/slack` | Quản lý cảnh báo giá | `list | <symbol> >|< <price> [stock|crypto] [--repeat]` |
| `/note` | `https://YOUR-WORKER.workers.dev/slack` | Tạo ghi chú hoặc xem danh sách | `<content> | list` |
| `/tcbs` | `https://YOUR-WORKER.workers.dev/slack` | Xem TCBS Top 10 holdings | `` |

## Sample Responses (Slack Markdown)

### /help

```
*Help*  |  _System_
`Info`

/help          Show available commands and usage
/pc            Get crypto price with 24h statistics
/ps            Get VnStock price with daily summary
/p             Get price, auto-detect crypto or VnStock
/psi           Get stock technical analysis snapshot
/pci           Get crypto technical analysis snapshot
/signal        Generate a trading signal for a symbol
/alert         Manage price alerts
/pf            Show portfolio summary for the current user
/note          Create a quick note or list recent notes
/tcbs          Show TCBS Top 10 current stock holdings
```

### /pc BTCUSDT

```
*BTCUSDT*  |  _Crypto Spot_
`Info`

Price      65,116.46
Change     -0.01%
High 24h   66,242.42
Low 24h    64,566.58
Volume     2,154,812
```

### /ps VNM

```
*VNM*  |  _VnStock_
`Info`

Close      72,400
Change     +1.45%
High       73,100
Low        71,800
Volume     15.20
```

### /p FPT

(Auto-detects as VnStock)

```
*FPT*  |  _VnStock_
`Info`

Close      142,500
Change     +2.34%
High       144,000
Low        141,200
Volume     8.75
```

### /pci BTCUSDT Hour4

```
*BTCUSDT [Hour4]*  |  _Analysis_
`Info`

Trend       Bullish
Momentum    Strong
Setup       Breakout
Signal      BUY
Confidence  85%
Entry       65,000
Stop Loss   63,500
Target 1    68,000
Target 2    71,000
```

### /psi VNM Day1

```
*VNM [Day1]*  |  _Analysis_
`Info`

Trend       Bearish
Momentum    Weak
Setup       Downtrend
Signal      SELL
Confidence  72%
Entry       72,400
Stop Loss   73,200
Target 1    70,000
Target 2    68,500
```

### /signal BTCUSDT

```
*BTCUSDT*  |  _Signal_
`Info`

Direction   BUY
Confidence  85%
Entry       65,000
Stop Loss   63,500
Target 1    68,000
Target 2    71,000
```

### /alert list

```
*Price Alerts*  |  _Alerts_
`Info`

BTCUSDT [Crypto]  Above 65000
FPT [Stock]       Below 140000
VNM [Stock]       Above 75000
```

### /pf

```
*Portfolio Summary*  |  _Portfolio_
`Info`

Crypto Growth [Stock]                 Crypto:3 Stock:5 Invested:125000 PnL:8500
Blue Chips [Stock]                    Crypto:0 Stock:12 Invested:250000 PnL:-3200
```

### /note list

```
*5 Notes*  |  _Notes_
`Info`

[2026-06-17]  Review BTC entry at 64k
[2026-06-16]  Check VNM dividend announcement
[2026-06-15]  Update portfolio allocation
[2026-06-14]  Research MEXC new listings
[2026-06-13]  Set alert for FPT at 140k
```

### /note Need to check VNM dividend date

```
*Need to check VNM dividend date*  |  _Notes_
`Info`

Status      Created
Id          8f3a1b2c-4d5e-6f7a-8b9c-0d1e2f3a4b5c
```

### /tcbs

```
*TCBS Top 10 Holdings*  |  _Market Data_
`Info`

#1         FPT
#2         VNM
#3         VHM
#4         MSN
#5         VCB
#6         HPG
#7         SSI
#8         MWG
#9         VIC
#10        GAS
```

---

## Current URLs

Worker health check:

```text
https://YOUR-WORKER.workers.dev/
```

Slack endpoint:

```text
https://YOUR-WORKER.workers.dev/slack
```

Backend URL:

```text
http://thaix.tryasp.net
```

## 1. Create or Open a Slack App

1. Go to:

```text
https://api.slack.com/apps
```

2. Create a new app or open the existing ThaiX app.
3. Select the workspace where the app will be installed.

## 2. Configure Cloudflare Worker Variables

Open Cloudflare Dashboard:

```text
Workers & Pages -> thaix -> Settings -> Variables and Secrets
```

Add or update these variables:

```text
BACKEND_URL=http://thaix.tryasp.net
SLACK_SIGNING_SECRET=<slack-signing-secret>
SLACK_BOT_TOKEN=<slack-bot-user-oauth-token>
BACKEND_CLIENT_ID=<thaix-m2m-client-id>
BACKEND_CLIENT_SECRET=<thaix-m2m-client-secret>
```

Recommended variable types:

```text
BACKEND_URL: Variable
SLACK_SIGNING_SECRET: Secret
SLACK_BOT_TOKEN: Secret
BACKEND_CLIENT_ID: Secret
BACKEND_CLIENT_SECRET: Secret
```

After saving variables, deploy the Worker again if Cloudflare asks for deployment.

## 3. Get Slack Signing Secret

In the Slack app settings:

```text
Basic Information -> App Credentials -> Signing Secret
```

Copy the signing secret and store it in Cloudflare as:

```text
SLACK_SIGNING_SECRET
```

The Worker uses this value to validate Slack requests.

## 4. Configure OAuth Scopes

In the Slack app settings:

```text
OAuth & Permissions -> Bot Token Scopes
```

Add these scopes:

```text
chat:write
app_mentions:read
commands
```

If the app needs to read channel context later, additional scopes may be needed, but the Worker in `cloudflare/_worker.js` only requires the scopes above for slash commands, app mentions, and bot replies.

Install or reinstall the app after changing scopes:

```text
OAuth & Permissions -> Install to Workspace
```

Copy the Bot User OAuth Token and store it in Cloudflare as:

```text
SLACK_BOT_TOKEN
```

The token usually starts with:

```text
xoxb-
```

## 5. Configure Slash Command

In the Slack app settings:

```text
Slash Commands -> Create New Command
```

Create one Slack slash command for each backend command above. Every command uses the same Request URL:

```text
Request URL: https://YOUR-WORKER.workers.dev/slack
```

Examples:

```text
Command: /help
Short Description: Show available commands
Usage Hint: [command]
```

```text
Command: /pc
Short Description: Get crypto price
Usage Hint: <symbol>
```

```text
Command: /ps
Short Description: Get stock price
Usage Hint: <symbol>
```

Important:

```text
If you want to use /help /pc /ps directly in Slack,
you must create each slash command separately in Slack.
All of them point to the same Request URL:
https://YOUR-WORKER.workers.dev/slack
```

Save the command.

If Slack asks to reinstall the app, reinstall it.

## 6. Configure App Mentions

In the Slack app settings:

```text
Event Subscriptions
```

Enable events and set the Request URL:

```text
https://YOUR-WORKER.workers.dev/slack
```

Slack will send a URL verification challenge. The Worker handles this automatically.

Then subscribe to this bot event:

```text
app_mention
```

Save changes and reinstall the app if Slack asks.

## 7. Verify Worker Health

Open this URL in a browser:

```text
https://YOUR-WORKER.workers.dev/
```

Expected response:

```json
{
  "status": "ok",
  "service": "thaix-slack-edge"
}
```

The timestamp may differ.

## 8. Test Slash Command

In Slack, run:

```text
/help
```

Expected flow:

1. Slack sends a signed POST request to `/slack`.
2. The Worker validates the Slack signature.
3. The Worker immediately responds to Slack with:

```text
Command received. Processing...
```

4. The Worker forwards the command to:

```text
http://thaix.tryasp.net/api/slack/commands/execute
```

5. The Worker sends the final response back to Slack through Slack `response_url`.

## 9. Test App Mention

Invite the bot to a channel, then send:

```text
@ThaiX help
```

Expected flow:

1. Slack sends an `app_mention` event to `/slack`.
2. The Worker validates the Slack signature.
3. The Worker strips the bot mention from the text.
4. The Worker forwards the command to:

```text
http://thaix.tryasp.net/api/slack/commands/execute
```

5. The Worker replies in the Slack thread using `chat.postMessage`.

## 10. Common Issues

### Slack Request URL verification fails

Check:

```text
SLACK_SIGNING_SECRET
https://YOUR-WORKER.workers.dev/slack
```

Also confirm the Worker has been deployed after setting variables.

### Slash command shows a timeout

The Worker should acknowledge Slack immediately. If timeout still happens, check Cloudflare Worker logs and confirm the endpoint is:

```text
https://YOUR-WORKER.workers.dev/slack
```

### Slack response says backend returned HTTP error

Check backend availability:

```text
http://thaix.tryasp.net
```

Then verify the backend command endpoint:

```text
http://thaix.tryasp.net/api/slack/commands/execute
```

### Backend returns authorization errors

Check these Cloudflare secrets:

```text
BACKEND_CLIENT_ID
BACKEND_CLIENT_SECRET
```

The Worker requests a machine-to-machine token from:

```text
http://thaix.tryasp.net/api/token/client-credentials
```

with this scope:

```text
BotCommand.Execute
```

### App mention does not respond

Check:

```text
SLACK_BOT_TOKEN
app_mentions:read scope
chat:write scope
Event Subscriptions -> app_mention
```

Also confirm the bot has been invited to the Slack channel.

## Information Needed Before Production

Confirm these values before production use:

```text
SLACK_SIGNING_SECRET
SLACK_BOT_TOKEN
BACKEND_CLIENT_ID
BACKEND_CLIENT_SECRET
```

Also confirm whether `BACKEND_URL` should stay on HTTP or move to HTTPS.