<?php
session_start();
require __DIR__ . '/helpers/chat_auth.php';
if (!function_exists('is_user_logged_in')) {
    // fallback: check session
    if (empty($_SESSION['user_id'])) {
        header('Location: login.php');
        exit;
    }
}
$current_user_id = $_SESSION['user_id'] ?? 0;
$csrf_token = $_SESSION['csrf_token'] ?? '';
?>
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width,initial-scale=1">
    <title>الدردشة - المسافر</title>
    <link rel="stylesheet" href="css/style.css">
    <link rel="stylesheet" href="css/chat.css">
    <style>/* small inline tweaks to match header */</style>
</head>
<body class="chat-page">
<div class="chat-container">
    <aside class="sidebar" id="conversations_panel">
        <!-- Conversations loaded via AJAX -->
    </aside>

    <section class="main" id="chat_panel">
        <div class="header">
            <button class="back" id="back_btn" onclick="showSidebar()">◀ رجوع</button>
            <div id="chat_title">اختر محادثة</div>
        </div>
        <div class="messages" id="messages_panel">
            <div style="text-align:center;color:#888">لا توجد رسائل لعرضها</div>
        </div>
        <div class="composer">
            <textarea id="message_input" rows="2" maxlength="2000" placeholder="اكتب رسالة..."></textarea>
            <button id="send_button" disabled>إرسال</button>
        </div>
    </section>
</div>

<script>
const currentUserId = <?= json_encode(intval($current_user_id)) ?>;
const csrfToken = <?= json_encode($csrf_token) ?>;
let currentConversationId = null;
let pollingInterval = null;

function escapeHtml(s){
    return String(s).replace(/[&<>"']/g, function(m){return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":"&#39;"}[m];});
}

async function fetchConversations(){
    try{
        const res = await fetch('api/conversations.php');
        const data = await res.json();
        renderConversations(data);
    }catch(e){console.error('conversations error',e)}
}

function renderConversations(list){
    const panel = document.getElementById('conversations_panel');
    panel.innerHTML = '';
    if(!Array.isArray(list)) return;
    // sort by last_message_at desc
    list.sort((a,b)=> new Date(b.last_message_at) - new Date(a.last_message_at));
    list.forEach(conv=>{
        const div = document.createElement('div');
        div.className = 'conv';
        div.dataset.cid = conv.id;
        div.innerHTML = `<div class="meta"><div class="name">${escapeHtml(conv.other_user_name || '---')}</div><div class="last">${escapeHtml(conv.last_message_at || '')}</div></div><div class="right-side">${conv.unread_count>0?'<span class="unread">'+conv.unread_count+'</span>':''}</div>`;
        div.addEventListener('click',()=>openConversation(conv.id, conv.other_user_name));
        panel.appendChild(div);
    });
}

async function openConversation(id, otherName){
    currentConversationId = id;
    document.getElementById('chat_title').textContent = otherName || 'محادثة';
    // mobile: hide sidebar
    if(window.innerWidth <= 768){
        document.getElementById('conversations_panel').classList.add('hidden');
    }
    await markRead(id);
    await fetchMessages(id);
}

async function markRead(id){
    try{
        await fetch('api/mark_read.php',{
            method:'POST',
            headers:{'Content-Type':'application/json'},
            body: JSON.stringify({conversation_id:id, csrf_token:csrfToken})
        });
        // update local UI by reloading conversations
        fetchConversations();
    }catch(e){console.error('mark_read',e)}
}

async function fetchMessages(id){
    if(!id) return;
    try{
        const res = await fetch('api/messages.php?conversation_id='+encodeURIComponent(id));
        const data = await res.json();
        renderMessages(data);
    }catch(e){console.error('messages',e)}
}

function renderMessages(list){
    const panel = document.getElementById('messages_panel');
    panel.innerHTML = '';
    if(!Array.isArray(list) || list.length===0){
        panel.innerHTML = '<div style="text-align:center;color:#888">لا توجد رسائل</div>';
        return;
    }
    list.forEach(msg=>{
        const row = document.createElement('div');
        const isMe = parseInt(msg.sender_id)==parseInt(currentUserId);
        row.className = 'message-row ' + (isMe? 'me':'other');
        const bubble = document.createElement('div');
        bubble.className = 'bubble';
        bubble.innerHTML = `<div class="text">${escapeHtml(msg.message)}</div><div class="time">${escapeHtml(msg.created_at)}</div>`;
        row.appendChild(bubble);
        panel.appendChild(row);
    });
    panel.scrollTop = panel.scrollHeight;
}

async function sendMessage(){
    const input = document.getElementById('message_input');
    const text = input.value.trim();
    if(!text || !currentConversationId) return;
    document.getElementById('send_button').disabled = true;
    try{
        const res = await fetch('api/send_message.php',{
            method:'POST',
            headers:{'Content-Type':'application/json'},
            body: JSON.stringify({conversation_id:currentConversationId, message:text, csrf_token:csrfToken})
        });
        const data = await res.json();
        if(data && data.success){
            input.value = '';
            document.getElementById('send_button').disabled = true;
            await fetchMessages(currentConversationId);
            await fetchConversations();
        }else{
            alert((data && data.error) || 'خطأ في الإرسال');
        }
    }catch(e){console.error('send',e);alert('خطأ في الإرسال')}
}

// Input validation and events
const inputEl = document.getElementById('message_input');
const sendBtn = document.getElementById('send_button');
inputEl.addEventListener('input', ()=>{
    const val = inputEl.value.trim();
    sendBtn.disabled = !val || val.length>2000;
});
sendBtn.addEventListener('click', sendMessage);
inputEl.addEventListener('keydown', (e)=>{
    if((e.key==='Enter' || e.keyCode===13) && !e.shiftKey){
        e.preventDefault(); sendMessage();
    }
});

function showSidebar(){
    document.getElementById('conversations_panel').classList.remove('hidden');
}

// Polling
function startPolling(){
    if(pollingInterval) clearInterval(pollingInterval);
    pollingInterval = setInterval(()=>{
        fetchConversations();
        if(currentConversationId) fetchMessages(currentConversationId);
    }, 5000);
}

// init
fetchConversations();
startPolling();
</script>
</body>
</html>
