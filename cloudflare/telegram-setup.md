# Telegram Setup for ThaiX Cloudflare Worker

This guide configures Telegram to send bot webhooks to the ThaiX Cloudflare Worker.

## Available Commands

| Command | Aliases | Syntax | Description |
| --- | --- | --- | --- |
| `/help` | `/h` | `/help [command]` | Hiển thị danh sách lệnh |
| `/pc` | `/pricec`, `/price-crypto` | `/pc <symbol>` | Lấy giá crypto + thống kê 24h |
| `/ps` | `/prices`, `/price-stock` | `/ps <symbol>` | Lấy giá chứng khoán Việt Nam |
| `/psi` | `/ta-s`, `/indicators-stock` | `/psi <symbol> [timeframe]` | Phân tích kỹ thuật chứng khoán |
| `/pci` | `/ta-c`, `/indicators-crypto` | `/pci <symbol> [timeframe]` | Phân tích kỹ thuật crypto |
| `/signal` | `/sg` | `/signal <symbol> [timeframe] [--stock\|--crypto]` | Tạo tín hiệu giao dịch |
| `/alert` | `/al` | `/alert list \| /alert <symbol> >\|< <price> [stock\|crypto] [--repeat]` | Quản lý cảnh báo giá |
| `/pf` | `/portfolio`, `/portfolio-summary` | `/pf` | Xem tóm tắt danh mục đầu tư |
| `/note` | `/n` | `/note <content> \| /note list` | Tạo ghi chú hoặc xem danh sách |
| `/tcbs` | `/tcbs-top10` | `/tcbs` | Xem TCBS Top 10 holdings |

## Sample Responses (Telegram HTML)

Telegram responses use `parse_mode=HTML`. All labels are left-aligned, values right-padded for consistent width.

### /help

<pre>
<b>Help</b>  |  <i>System</i>
<code>Info</code>

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
</pre>

### /pc BTCUSDT

<pre>
<b>BTCUSDT</b>  |  <i>Crypto Spot</i>
<code>Info</code>

Price      65,116.46
Change     -0.01%
High 24h   66,242.42
Low 24h    64,566.58
Volume     2,154,812
</pre>

### /ps VNM

<pre>
<b>VNM</b>  |  <i>VnStock</i>
<code>Info</code>

Close      72,400
Change     +1.45%
High       73,100
Low        71,800
Volume     15.20
</pre>

### /p FPT

(Auto-detects as VnStock)

<pre>
<b>FPT</b>  |  <i>VnStock</i>
<code>Info</code>

Close      142,500
Change     +2.34%
High       144,000
Low        141,200
Volume     8.75
</pre>

### /pci BTCUSDT Hour4

<pre>
<b>BTCUSDT [Hour4]</b>  |  <i>Analysis</i>
<code>Info</code>

Trend       Bullish
Momentum    Strong
Setup       Breakout
Signal      BUY
Confidence  85%
Entry       65,000
Stop Loss   63,500
Target 1    68,000
Target 2    71,000
</pre>

### /psi VNM Day1

<pre>
<b>VNM [Day1]</b>  |  <i>Analysis</i>
<code>Info</code>

Trend       Bearish
Momentum    Weak
Setup       Downtrend
Signal      SELL
Confidence  72%
Entry       72,400
Stop Loss   73,200
Target 1    70,000
Target 2    68,500
</pre>

### /signal BTCUSDT

<pre>
<b>BTCUSDT</b>  |  <i>Signal</i>
<code>Info</code>

Direction   BUY
Confidence  85%
Entry       65,000
Stop Loss   63,500
Target 1    68,000
Target 2    71,000
</pre>

### /alert list

<pre>
<b>Price Alerts</b>  |  <i>Alerts</i>
<code>Info</code>

BTCUSDT [Crypto]  Above 65000
FPT [Stock]       Below 140000
VNM [Stock]       Above 75000
</pre>

### /pf

<pre>
<b>Portfolio Summary</b>  |  <i>Portfolio</i>
<code>Info</code>

Crypto Growth [Stock]                 Crypto:3 Stock:5 Invested:125000 PnL:8500
Blue Chips [Stock]                    Crypto:0 Stock:12 Invested:250000 PnL:-3200
</pre>

### /note list

<pre>
<b>5 Notes</b>  |  <i>Notes</i>
<code>Info</code>

[2026-06-17]  Review BTC entry at 64k
[2026-06-16]  Check VNM dividend announcement
[2026-06-15]  Update portfolio allocation
[2026-06-14]  Research MEXC new listings
[2026-06-13]  Set alert for FPT at 140k
</pre>

### /note Need to check VNM dividend date

<pre>
<b>Need to check VNM dividend date</b>  |  <i>Notes</i>
<code>Info</code>

Status      Created
Id          8f3a1b2c-4d5e-6f7a-8b9c-0d1e2f3a4b5c
</pre>

### /tcbs

<pre>
<b>TCBS Top 10 Holdings</b>  |  <i>Market Data</i>
<code>Info</code>

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
</pre>

---

## Current URLs

Worker health check:

```text
https://YOUR-WORKER.workers.dev/
```

Telegram webhook endpoint:

```text
https://YOUR-WORKER.workers.dev/telegram
```

Backend URL:

```text
http://thaix.tryasp.net
```

## 1. Create a Telegram Bot

1. Open Telegram and start a chat with `@BotFather`.
2. Send:

```text
/newbot
```

3. Follow the prompts to choose:
   - Bot display name
   - Bot username, ending with `bot`
4. Copy the bot token returned by BotFather.

The token usually looks like this:

```text
1234567890:AAExampleTokenValue
```

## 2. Configure Cloudflare Worker Variables

Open Cloudflare Dashboard:

```text
Workers & Pages -> thaix -> Settings -> Variables and Secrets
```

Add or update these variables:

```text
BACKEND_URL=http://thaix.tryasp.net
TELEGRAM_BOT_TOKEN=<telegram-bot-token-from-botfather>
TELEGRAM_WEBHOOK_SECRET=<random-secret-string>
BACKEND_CLIENT_ID=<thaix-m2m-client-id>
BACKEND_CLIENT_SECRET=<thaix-m2m-client-secret>
```

Recommended variable types:

```text
BACKEND_URL: Variable
TELEGRAM_BOT_TOKEN: Secret
TELEGRAM_WEBHOOK_SECRET: Secret
BACKEND_CLIENT_ID: Secret
BACKEND_CLIENT_SECRET: Secret
```

Use a long random value for `TELEGRAM_WEBHOOK_SECRET`. Example format:

```text
thaix-telegram-2026-change-this-to-a-long-random-value
```

After saving variables, deploy the Worker again if Cloudflare asks for deployment.

## 3. Verify Worker Health

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

## 4. Register Telegram Webhook

Run this PowerShell command after replacing the token and secret:

```powershell
$botToken = "<TELEGRAM_BOT_TOKEN>"
$webhookUrl = "https://YOUR-WORKER.workers.dev/telegram"
$secret = "<TELEGRAM_WEBHOOK_SECRET>"

Invoke-RestMethod `
  -Method Post `
  -Uri "https://api.telegram.org/bot$botToken/setWebhook" `
  -ContentType "application/json" `
  -Body (@{
    url = $webhookUrl
    secret_token = $secret
  } | ConvertTo-Json)
```

Expected result:

```json
{
  "ok": true,
  "result": true,
  "description": "Webhook was set"
}
```

## 5. Check Webhook Status

Run:

```powershell
$botToken = "<TELEGRAM_BOT_TOKEN>"

Invoke-RestMethod `
  -Method Get `
  -Uri "https://api.telegram.org/bot$botToken/getWebhookInfo"
```

Check that:

```text
url = https://YOUR-WORKER.workers.dev/telegram
pending_update_count is low or zero
last_error_message is empty
```

## 6. Test the Bot

Open Telegram and send a message to the bot.

The Worker will:

1. Receive Telegram update at `/telegram`.
2. Validate `X-Telegram-Bot-Api-Secret-Token` when `TELEGRAM_WEBHOOK_SECRET` is configured.
3. Forward the command to:

```text
http://thaix.tryasp.net/api/telegram/webhook
```

4. Send the backend response back to the Telegram chat.

## 7. Common Issues

### Worker returns 500

Check Cloudflare variables:

```text
TELEGRAM_BOT_TOKEN
BACKEND_URL
BACKEND_CLIENT_ID
BACKEND_CLIENT_SECRET
```

### Worker returns 401

The webhook secret sent by Telegram does not match `TELEGRAM_WEBHOOK_SECRET`.

Fix by registering the webhook again with the same secret stored in Cloudflare.

### Telegram receives no response

Check:

```text
https://api.telegram.org/bot<TELEGRAM_BOT_TOKEN>/getWebhookInfo
```

Then verify the backend endpoint is reachable:

```text
http://thaix.tryasp.net/api/telegram/webhook
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

## 8. Disable Telegram Webhook

Run:

```powershell
$botToken = "<TELEGRAM_BOT_TOKEN>"

Invoke-RestMethod `
  -Method Post `
  -Uri "https://api.telegram.org/bot$botToken/deleteWebhook"
```

## Information Needed Before Production

Confirm these values before production use:

```text
TELEGRAM_BOT_TOKEN
TELEGRAM_WEBHOOK_SECRET
BACKEND_CLIENT_ID
BACKEND_CLIENT_SECRET
```

Also confirm whether `BACKEND_URL` should stay on HTTP or move to HTTPS.
