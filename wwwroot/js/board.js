// ── State ──────────────────────────────────────────────────────────────────
const boardId  = document.getElementById('board-app').dataset.boardId;
let listsData  = [];   // ListWithCardsDto[]
let dragState  = null; // { cardId, fromListId, fromPosition }

// ── Bootstrap modal instance ───────────────────────────────────────────────
let cardModal = null;

// ── Init ───────────────────────────────────────────────────────────────────
(async function init() {
    if (!requireAuth()) return;
    cardModal = new bootstrap.Modal(document.getElementById('cardDetailModal'));
    try {
        await loadBoard();
        connectSignalR();
    } catch (e) {
        alert('Erreur de chargement : ' + e.message);
    }
})();

// ── Load board ─────────────────────────────────────────────────────────────
async function loadBoard() {
    const [board, lists] = await Promise.all([
        velcroApi(`/boards/${boardId}`),
        velcroApi(`/boards/${boardId}/lists`)
    ]);
    if (!board || !lists) return;
    document.getElementById('board-name').textContent = board.name;
    document.getElementById('board-app').style.background = board.backgroundColor || '#0079bf';
    listsData = lists;
    renderAllLists();
}

// ── Render ─────────────────────────────────────────────────────────────────
function renderAllLists() {
    const row = document.getElementById('lists-row');
    const addCol = document.getElementById('add-list-col');
    // Remove existing list columns (keep add-list-col)
    row.querySelectorAll('.list-col').forEach(el => el.remove());
    listsData.forEach(l => row.insertBefore(buildListEl(l), addCol));
}

function buildListEl(list) {
    const col = document.createElement('div');
    col.className = 'list-col';
    col.dataset.listId = list.id;
    col.innerHTML = `
        <div class="list-header">
            <input class="list-name-input" value="${escHtml(list.name)}" data-list-id="${list.id}">
            <button class="btn btn-sm btn-link text-danger p-0 ms-1 btn-del-list" data-list-id="${list.id}" title="Supprimer">✕</button>
        </div>
        <div class="cards-container" data-list-id="${list.id}"></div>
        <div class="add-card-zone">
            <button class="btn-add-card" data-list-id="${list.id}">+ Ajouter une carte</button>
            <div class="add-card-form" id="add-card-form-${list.id}">
                <input type="text" class="form-control form-control-sm mb-1 new-card-input" placeholder="Titre de la carte">
                <div class="d-flex gap-1">
                    <button class="btn btn-primary btn-sm confirm-add-card" data-list-id="${list.id}">Ajouter</button>
                    <button class="btn btn-sm cancel-add-card" data-list-id="${list.id}">✕</button>
                </div>
            </div>
        </div>`;

    const container = col.querySelector('.cards-container');
    list.cards.forEach(c => container.appendChild(buildCardEl(c)));
    setupDropZone(container);
    return col;
}

function buildCardEl(card) {
    const el = document.createElement('div');
    el.className = 'card-item';
    el.setAttribute('draggable', 'true');
    el.dataset.cardId   = card.id;
    el.dataset.listId   = card.listId;
    el.dataset.position = card.position;

    const dueHtml = card.dueDate
        ? `<div class="card-due ${new Date(card.dueDate) < new Date() ? 'overdue' : ''}">📅 ${formatDate(card.dueDate)}</div>`
        : '';
    el.innerHTML = `<div class="card-title-text">${escHtml(card.title)}</div>${dueHtml}`;

    el.addEventListener('click', () => openCardModal(card.id));
    el.addEventListener('dragstart', e => {
        dragState = { cardId: card.id, fromListId: card.listId, fromPosition: card.position };
        e.dataTransfer.effectAllowed = 'move';
        setTimeout(() => el.classList.add('dragging'), 0);
    });
    el.addEventListener('dragend', () => {
        el.classList.remove('dragging');
        dragState = null;
        document.querySelectorAll('.drop-placeholder').forEach(p => p.remove());
        document.querySelectorAll('.drag-over').forEach(c => c.classList.remove('drag-over'));
    });
    return el;
}

// ── Drop zones ─────────────────────────────────────────────────────────────
function setupDropZone(container) {
    container.addEventListener('dragover', e => {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        container.classList.add('drag-over');

        // Show placeholder at right position
        document.querySelectorAll('.drop-placeholder').forEach(p => p.remove());
        const afterEl = getDragAfterElement(container, e.clientY);
        const placeholder = document.createElement('div');
        placeholder.className = 'drop-placeholder';
        if (afterEl) container.insertBefore(placeholder, afterEl);
        else container.appendChild(placeholder);
    });

    container.addEventListener('dragleave', e => {
        if (!container.contains(e.relatedTarget)) {
            container.classList.remove('drag-over');
            container.querySelectorAll('.drop-placeholder').forEach(p => p.remove());
        }
    });

    container.addEventListener('drop', async e => {
        e.preventDefault();
        if (!dragState) return;
        container.classList.remove('drag-over');
        document.querySelectorAll('.drop-placeholder').forEach(p => p.remove());

        const targetListId = container.dataset.listId;
        const afterEl      = getDragAfterElement(container, e.clientY);
        const cards        = [...container.querySelectorAll('.card-item:not(.dragging)')];
        let newPosition    = afterEl ? cards.indexOf(afterEl) : cards.length;
        if (newPosition < 0) newPosition = cards.length;

        try {
            await velcroApi(`/cards/${dragState.cardId}/move`, 'PATCH', { targetListId, newPosition });
        } catch (err) {
            alert(err.message);
        }
    });
}

function getDragAfterElement(container, y) {
    const els = [...container.querySelectorAll('.card-item:not(.dragging)')];
    return els.reduce((closest, el) => {
        const box = el.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;
        if (offset < 0 && offset > closest.offset) return { offset, element: el };
        return closest;
    }, { offset: Number.NEGATIVE_INFINITY }).element;
}

// ── Card modal ─────────────────────────────────────────────────────────────
async function openCardModal(cardId) {
    try {
        const card = await velcroApi(`/cards/${cardId}`);
        if (!card) return;

        document.getElementById('modal-card-title').value    = card.title;
        document.getElementById('modal-card-desc').value     = card.description || '';
        document.getElementById('modal-card-due').value      = card.dueDate ? card.dueDate.slice(0, 10) : '';
        document.getElementById('modal-card-id').value       = card.id;

        const commentsList = document.getElementById('modal-comments-list');
        commentsList.innerHTML = '';
        (card.comments || []).forEach(co => commentsList.appendChild(buildCommentEl(co, card.id)));

        document.getElementById('modal-new-comment').value = '';
        cardModal.show();

        document.getElementById('btn-save-card').onclick = () => saveCard(card);
        document.getElementById('btn-delete-card').onclick = () => deleteCard(card.id);
        document.getElementById('btn-add-comment').onclick = () => addComment(card.id, card.boardId);
    } catch (e) {
        alert(e.message);
    }
}

function buildCommentEl(comment, cardId) {
    const li = document.createElement('div');
    li.className = 'd-flex justify-content-between align-items-start mb-2 p-2 bg-light rounded';
    li.dataset.commentId = comment.id;
    li.innerHTML = `
        <div>
            <strong class="small">${escHtml(comment.authorUsername)}</strong>
            <span class="text-muted small ms-2">${formatDate(comment.createdAt)}</span>
            <p class="mb-0 mt-1">${escHtml(comment.content)}</p>
        </div>
        <button class="btn btn-sm btn-link text-danger p-0 btn-del-comment" data-comment-id="${comment.id}" data-card-id="${cardId}">✕</button>`;
    li.querySelector('.btn-del-comment').addEventListener('click', () => deleteComment(cardId, comment.id));
    return li;
}

async function saveCard(card) {
    const title       = document.getElementById('modal-card-title').value.trim();
    const description = document.getElementById('modal-card-desc').value.trim();
    const dueVal      = document.getElementById('modal-card-due').value;
    const dueDate     = dueVal ? new Date(dueVal).toISOString() : null;
    if (!title) return;
    try {
        await velcroApi(`/cards/${card.id}`, 'PUT', { title, description, dueDate });
    } catch (e) { alert(e.message); }
}

async function deleteCard(cardId) {
    if (!confirm('Supprimer cette carte ?')) return;
    try {
        await velcroApi(`/cards/${cardId}`, 'DELETE');
        cardModal.hide();
    } catch (e) { alert(e.message); }
}

async function addComment(cardId, boardId) {
    const content = document.getElementById('modal-new-comment').value.trim();
    if (!content) return;
    try {
        const comment = await velcroApi(`/cards/${cardId}/comments`, 'POST', { content });
        document.getElementById('modal-comments-list').appendChild(buildCommentEl(comment, cardId));
        document.getElementById('modal-new-comment').value = '';
    } catch (e) { alert(e.message); }
}

async function deleteComment(cardId, commentId) {
    if (!confirm('Supprimer ce commentaire ?')) return;
    try {
        await velcroApi(`/cards/${cardId}/comments/${commentId}`, 'DELETE');
        document.querySelector(`[data-comment-id="${commentId}"]`)?.remove();
    } catch (e) { alert(e.message); }
}

// ── Add card form ──────────────────────────────────────────────────────────
document.getElementById('lists-row').addEventListener('click', async e => {
    // Show add-card form
    const btnAdd = e.target.closest('.btn-add-card');
    if (btnAdd) {
        const listId = btnAdd.dataset.listId;
        const form   = document.getElementById(`add-card-form-${listId}`);
        form.classList.add('open');
        form.querySelector('.new-card-input').focus();
    }

    // Cancel add-card
    const btnCancel = e.target.closest('.cancel-add-card');
    if (btnCancel) {
        const listId = btnCancel.dataset.listId;
        const form   = document.getElementById(`add-card-form-${listId}`);
        form.classList.remove('open');
        form.querySelector('.new-card-input').value = '';
    }

    // Confirm add-card
    const btnConfirm = e.target.closest('.confirm-add-card');
    if (btnConfirm) {
        const listId = btnConfirm.dataset.listId;
        const form   = document.getElementById(`add-card-form-${listId}`);
        const title  = form.querySelector('.new-card-input').value.trim();
        if (!title) return;
        try {
            await velcroApi('/cards', 'POST', { listId, title });
            form.classList.remove('open');
            form.querySelector('.new-card-input').value = '';
        } catch (err) { alert(err.message); }
    }

    // Delete list
    const btnDel = e.target.closest('.btn-del-list');
    if (btnDel) {
        if (!confirm('Supprimer cette liste et toutes ses cartes ?')) return;
        const listId = btnDel.dataset.listId;
        try {
            await velcroApi(`/lists/${listId}`, 'DELETE');
        } catch (err) { alert(err.message); }
    }
});

// Add list form
document.getElementById('btn-add-list').addEventListener('click', () => {
    document.getElementById('add-list-form').classList.add('open');
    document.getElementById('new-list-name').focus();
});
document.getElementById('cancel-add-list').addEventListener('click', () => {
    document.getElementById('add-list-form').classList.remove('open');
    document.getElementById('new-list-name').value = '';
});
document.getElementById('confirm-add-list').addEventListener('click', async () => {
    const name = document.getElementById('new-list-name').value.trim();
    if (!name) return;
    try {
        await velcroApi('/lists', 'POST', { boardId, name });
        document.getElementById('add-list-form').classList.remove('open');
        document.getElementById('new-list-name').value = '';
    } catch (e) { alert(e.message); }
});
document.getElementById('new-list-name').addEventListener('keydown', e => {
    if (e.key === 'Enter') document.getElementById('confirm-add-list').click();
    if (e.key === 'Escape') document.getElementById('cancel-add-list').click();
});

// ── SignalR ────────────────────────────────────────────────────────────────
function connectSignalR() {
    const token = getToken();
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/board?access_token=${token}`)
        .withAutomaticReconnect()
        .build();

    connection.on('CardCreated', card => {
        const list = listsData.find(l => l.id === card.listId);
        if (list) {
            list.cards.push(card);
            const container = document.querySelector(`.cards-container[data-list-id="${card.listId}"]`);
            container?.appendChild(buildCardEl(card));
        }
    });

    connection.on('CardUpdated', card => {
        const el = document.querySelector(`.card-item[data-card-id="${card.id}"]`);
        if (!el) return;
        const newEl = buildCardEl(card);
        el.replaceWith(newEl);
        // Update in listsData
        for (const list of listsData) {
            const idx = list.cards.findIndex(c => c.id === card.id);
            if (idx !== -1) { list.cards[idx] = card; break; }
        }
    });

    connection.on('CardDeleted', cardId => {
        document.querySelector(`.card-item[data-card-id="${cardId}"]`)?.remove();
        for (const list of listsData) {
            const idx = list.cards.findIndex(c => c.id === cardId);
            if (idx !== -1) { list.cards.splice(idx, 1); break; }
        }
    });

    connection.on('CardMoved', card => {
        // Remove from old position in DOM
        const oldEl = document.querySelector(`.card-item[data-card-id="${card.id}"]`);
        oldEl?.remove();
        // Remove from old list in state
        for (const list of listsData) {
            const idx = list.cards.findIndex(c => c.id === card.id);
            if (idx !== -1) { list.cards.splice(idx, 1); break; }
        }
        // Insert in new list
        const list = listsData.find(l => l.id === card.listId);
        if (list) {
            list.cards.splice(card.position, 0, card);
            list.cards.forEach((c, i) => c.position = i);
            const container = document.querySelector(`.cards-container[data-list-id="${card.listId}"]`);
            if (container) {
                const newEl = buildCardEl(card);
                const siblings = [...container.querySelectorAll('.card-item')];
                const after   = siblings.find(s => parseInt(s.dataset.position) > card.position);
                if (after) container.insertBefore(newEl, after);
                else container.appendChild(newEl);
                // Re-index data-position attributes
                [...container.querySelectorAll('.card-item')].forEach((s, i) => s.dataset.position = i);
            }
        }
    });

    connection.on('ListCreated', list => {
        list.cards = list.cards || [];
        listsData.push(list);
        const row    = document.getElementById('lists-row');
        const addCol = document.getElementById('add-list-col');
        row.insertBefore(buildListEl(list), addCol);
    });

    connection.on('ListUpdated', list => {
        const input = document.querySelector(`.list-name-input[data-list-id="${list.id}"]`);
        if (input) input.value = list.name;
        const l = listsData.find(x => x.id === list.id);
        if (l) l.name = list.name;
    });

    connection.on('ListDeleted', listId => {
        document.querySelector(`.list-col[data-list-id="${listId}"]`)?.remove();
        listsData = listsData.filter(l => l.id !== listId);
    });

    connection.on('BoardUpdated', board => {
        document.getElementById('board-name').textContent = board.name;
        document.getElementById('board-app').style.background = board.backgroundColor || '#0079bf';
    });

    connection.on('BoardDeleted', () => {
        alert('Ce board a été supprimé.');
        window.location.href = '/';
    });

    connection.start()
        .then(() => connection.invoke('JoinBoard', boardId))
        .catch(err => console.error('SignalR error:', err));
}
