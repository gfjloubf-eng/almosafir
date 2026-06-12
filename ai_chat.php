<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
<title>مساعد ذكي - المسافر</title>
<link rel="stylesheet" href="css/style.css">
<script>
async function askAI() {
    const question = document.getElementById('question').value;
    const response = await fetch('https://api.openai.com/v1/chat/completions', {
        method: 'POST',
        headers: {
            'Authorization': 'Bearer YOUR_OPENAI_KEY', // Add your key
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            model: 'gpt-4o-mini',
            messages: [{role: 'user', content: `سؤال عن تطبيق المسافر: ${question}`}],
            max_tokens: 300
        })
    });
    const data = await response.json();
    document.getElementById('answer').innerText = data.choices[0].message.content;
}
</script>
</head>
<body>
<div class="container">
<h2>🤖 مساعد ذكي AI</h2>
<input id="question" placeholder="اسأل عن الرحلات أو الحجز...">
<button onclick="askAI()">سؤال</button>
<div id="answer" style="margin-top:20px; padding:20px; background:#f0f8ff;"></div>
</div>
</body>
</html>

