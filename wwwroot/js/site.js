// Auto-resize textarea as user types
function autoResize(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, 120) + 'px';
}

// Scroll chat to bottom smoothly
function scrollToBottom(container) {
    container.scrollTo({ top: container.scrollHeight, behavior: 'smooth' });
}

// Show typing indicator
function showTypingIndicator(container) {
    const indicator = document.createElement('div');
    indicator.className = 'chat-message assistant';
    indicator.id = 'typing-indicator';
    indicator.innerHTML = `
        <span class="message-label">Assistant</span>
        <div class="typing-indicator">
            <span></span><span></span><span></span>
        </div>`;
    container.appendChild(indicator);
    scrollToBottom(container);
}

// Remove typing indicator
function hideTypingIndicator() {
    const indicator = document.getElementById('typing-indicator');
    if (indicator) indicator.remove();
}

// Append a message bubble to the chat
function appendMessage(container, role, text) {
    const div = document.createElement('div');
    div.className = `chat-message ${role}`;
    div.innerHTML = `
        <span class="message-label">${role === 'user' ? 'You' : 'Assistant'}</span>
        <div class="message-bubble">${text.replace(/\n/g, '<br>')}</div>`;
    container.appendChild(div);
    scrollToBottom(container);
}