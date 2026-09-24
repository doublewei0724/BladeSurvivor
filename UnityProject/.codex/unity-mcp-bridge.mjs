import readline from 'node:readline';
import { randomUUID } from 'node:crypto';

const endpoint = 'http://127.0.0.1:8080/';
const input = readline.createInterface({ input: process.stdin, crlfDelay: Infinity });

for await (const line of input) {
  if (!line.trim()) continue;
  let request;
  try {
    request = JSON.parse(line);
    if (request.id === undefined) continue;
    async function send() {
      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json, text/event-stream',
        },
        body: JSON.stringify({ ...request, id: randomUUID() }),
      });
      if (!response.ok) throw new Error(`Unity MCP returned HTTP ${response.status}`);
      return response.json();
    }
    let result = await send();
    result.id = request.id;
    process.stdout.write(`${JSON.stringify(result)}\n`);
  } catch (error) {
    if (request?.id !== undefined) {
      process.stdout.write(`${JSON.stringify({
        jsonrpc: '2.0',
        id: request.id,
        error: { code: -32603, message: String(error) },
      })}\n`);
    } else {
      process.stderr.write(`${error}\n`);
    }
  }
}
