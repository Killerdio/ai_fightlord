document.addEventListener('DOMContentLoaded', () => {
    // Window Management
    const desktop = document.getElementById('desktop');
    const taskbarApps = document.getElementById('taskbar-apps');
    const windows = document.querySelectorAll('.window');
    let zIndexCounter = 100;
    let activeWindow = null;

    function openGameWindow() {
        const gameWindow = document.getElementById('game-window');
        if (gameWindow.style.display === 'none') {
            gameWindow.style.display = 'flex';
            centerWindow(gameWindow);
            activateWindow(gameWindow);
            addToTaskbar(gameWindow, 'FightLord Game');
        } else {
            if (gameWindow.classList.contains('minimized')) {
                restoreWindow(gameWindow);
            }
            activateWindow(gameWindow);
        }
    }

    window.openGameWindow = openGameWindow; // Expose to global scope for onclick

    function centerWindow(win) {
        const x = (window.innerWidth - win.offsetWidth) / 2;
        const y = (window.innerHeight - win.offsetHeight) / 2;
        win.style.left = `${Math.max(0, x)}px`;
        win.style.top = `${Math.max(0, y)}px`;
    }

    function activateWindow(win) {
        zIndexCounter++;
        win.style.zIndex = zIndexCounter;
        activeWindow = win;
        
        // Update taskbar active state
        document.querySelectorAll('.taskbar-item').forEach(item => {
            item.classList.remove('active');
            if (item.dataset.target === win.id) {
                item.classList.add('active');
            }
        });
    }

    function minimizeWindow(win) {
        win.classList.add('minimized');
        activeWindow = null;
    }

    function restoreWindow(win) {
        win.classList.remove('minimized');
        activateWindow(win);
    }

    function maximizeWindow(win) {
        win.classList.toggle('maximized');
    }

    function closeWindow(win) {
        win.style.display = 'none';
        removeFromTaskbar(win.id);
        if (activeWindow === win) {
            activeWindow = null;
        }
    }

    function addToTaskbar(win, title) {
        const existingItem = document.querySelector(`.taskbar-item[data-target="${win.id}"]`);
        if (existingItem) return;

        const item = document.createElement('div');
        item.className = 'taskbar-item';
        item.dataset.target = win.id;
        item.textContent = title;
        item.onclick = () => {
            if (win.classList.contains('minimized')) {
                restoreWindow(win);
            } else if (activeWindow === win) {
                minimizeWindow(win);
            } else {
                activateWindow(win);
            }
        };
        taskbarApps.appendChild(item);
    }

    function removeFromTaskbar(windowId) {
        const item = document.querySelector(`.taskbar-item[data-target="${windowId}"]`);
        if (item) {
            item.remove();
        }
    }

    // Window Dragging
    let isDragging = false;
    let startX, startY, initialLeft, initialTop;

    windows.forEach(win => {
        const header = win.querySelector('.window-header');
        const minimizeBtn = win.querySelector('.minimize-btn');
        const maximizeBtn = win.querySelector('.maximize-btn');
        const closeBtn = win.querySelector('.close-btn');

        win.addEventListener('mousedown', () => activateWindow(win));

        header.addEventListener('mousedown', (e) => {
            if (e.target.tagName === 'BUTTON') return;
            if (win.classList.contains('maximized')) return;
            
            activateWindow(win); // Ensure window is active before dragging

            isDragging = true;
            startX = e.clientX;
            startY = e.clientY;
            initialLeft = win.offsetLeft;
            initialTop = win.offsetTop;

            document.addEventListener('mousemove', onDrag);
            document.addEventListener('mouseup', stopDrag);
        });

        minimizeBtn.onclick = () => minimizeWindow(win);
        maximizeBtn.onclick = () => maximizeWindow(win);
        closeBtn.onclick = () => closeWindow(win);
    });

    function onDrag(e) {
        if (!isDragging) return;
        const dx = e.clientX - startX;
        const dy = e.clientY - startY;
        
        if (activeWindow) {
            activeWindow.style.left = `${initialLeft + dx}px`;
            activeWindow.style.top = `${initialTop + dy}px`;
        }
    }

    function stopDrag() {
        isDragging = false;
        document.removeEventListener('mousemove', onDrag);
        document.removeEventListener('mouseup', stopDrag);
    }

    // Clock
    function updateClock() {
        const now = new Date();
        document.getElementById('clock').textContent = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    setInterval(updateClock, 1000);
    updateClock();

    // Game Logic
    const startBtn = document.getElementById('start-game-btn');
    const bidBtn = document.getElementById('bid-btn');
    const playBtn = document.getElementById('play-btn');
    const passBtn = document.getElementById('pass-btn');
    const statusDiv = document.getElementById('game-status');
    const handDiv = document.getElementById('player-hand');

    let playerId = 'player1'; // Mock ID
    let selectedCards = [];

    startBtn.onclick = async () => {
        statusDiv.textContent = 'Game Started! Your turn to bid.';
        // In a real app, call API to start game
        // For now, mock a hand
        renderHand([
            { id: 1, rank: '3', suit: '♠' },
            { id: 2, rank: '4', suit: '♥' },
            { id: 3, rank: '5', suit: '♣' },
            { id: 4, rank: '6', suit: '♦' },
            { id: 5, rank: '7', suit: '♠' },
            { id: 6, rank: '8', suit: '♥' },
            { id: 7, rank: '9', suit: '♣' },
            { id: 8, rank: '10', suit: '♦' },
            { id: 9, rank: 'J', suit: '♠' },
            { id: 10, rank: 'Q', suit: '♥' },
            { id: 11, rank: 'K', suit: '♣' },
            { id: 12, rank: 'A', suit: '♦' },
            { id: 13, rank: '2', suit: '♠' },
        ]);
        startBtn.disabled = true;
        bidBtn.disabled = false;
    };

    bidBtn.onclick = async () => {
        const score = prompt('Enter bid score (1, 2, or 3):');
        if (!score) return;

        try {
            const response = await fetch('/api/v1/game/bid', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId, score: parseInt(score) })
            });
            
            if (response.ok) {
                statusDiv.textContent = `You bid ${score}. Waiting for others...`;
                bidBtn.disabled = true;
                // Enable play/pass for demo purposes after a "delay"
                setTimeout(() => {
                    statusDiv.textContent = 'Your turn to play!';
                    playBtn.disabled = false;
                    passBtn.disabled = false;
                }, 2000);
            } else {
                alert('Bid failed');
            }
        } catch (error) {
            console.error('Error:', error);
            // Mock success for demo if API fails (since API might not be fully ready)
            statusDiv.textContent = `(Mock) You bid ${score}.`;
             bidBtn.disabled = true;
             playBtn.disabled = false;
             passBtn.disabled = false;
        }
    };

    playBtn.onclick = async () => {
        if (selectedCards.length === 0) {
            alert('Select cards to play!');
            return;
        }

        const cardIds = selectedCards.map(c => c.id);
        
        try {
            const response = await fetch('/api/v1/game/play', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId, cardIds })
            });

            if (response.ok) {
                statusDiv.textContent = 'Cards played!';
                // Remove played cards
                selectedCards.forEach(c => {
                    const el = document.querySelector(`.card[data-id="${c.id}"]`);
                    if (el) el.remove();
                });
                selectedCards = [];
            } else {
                alert('Invalid move!');
            }
        } catch (error) {
            console.error('Error:', error);
             // Mock success
            statusDiv.textContent = '(Mock) Cards played!';
            selectedCards.forEach(c => {
                const el = document.querySelector(`.card[data-id="${c.id}"]`);
                if (el) el.remove();
            });
            selectedCards = [];
        }
    };

    passBtn.onclick = async () => {
        try {
            const response = await fetch('/api/v1/game/pass', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId })
            });

            if (response.ok) {
                statusDiv.textContent = 'You passed.';
            } else {
                alert('Cannot pass!');
            }
        } catch (error) {
             console.error('Error:', error);
             statusDiv.textContent = '(Mock) You passed.';
        }
    };

    function renderHand(cards) {
        handDiv.innerHTML = '';
        cards.forEach(card => {
            const el = document.createElement('div');
            el.className = 'card';
            el.textContent = `${card.rank} ${card.suit}`;
            el.dataset.id = card.id;
            
            if (['♥', '♦'].includes(card.suit)) {
                el.style.color = 'red';
            } else {
                el.style.color = 'black';
            }

            el.onclick = () => toggleCardSelection(el, card);
            handDiv.appendChild(el);
        });
    }

    function toggleCardSelection(el, card) {
        el.classList.toggle('selected');
        if (el.classList.contains('selected')) {
            selectedCards.push(card);
        } else {
            selectedCards = selectedCards.filter(c => c.id !== card.id);
        }
    }
});