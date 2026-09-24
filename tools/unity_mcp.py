"""Local MCP probe; always use a unique ID because UnityMCP caches IDs."""
import json, sys, uuid, urllib.request

method = sys.argv[1]
params = json.loads(sys.argv[2]) if len(sys.argv) > 2 else {}
payload = dict(jsonrpc='2.0', id=str(uuid.uuid4()), method=method, params=params)
req = urllib.request.Request('http://127.0.0.1:8080/', json.dumps(payload).encode(),
    {'Content-Type':'application/json', 'Accept':'application/json, text/event-stream'})
with urllib.request.urlopen(req, timeout=90) as response:
    data = json.load(response)
for item in data.get('result', {}).get('content', []):
    if item.get('type') == 'text':
        try: item['parsed'] = json.loads(item['text']); del item['text']
        except ValueError: pass
print(json.dumps(data, ensure_ascii=False, indent=2))
