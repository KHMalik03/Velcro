document.addEventListener('DOMContentLoaded', async () => {
    if (!requireAuth()) return;

    const user = getUser();
    if (user) document.getElementById('greeting').textContent = `Bonjour, ${escHtml(user.username)} !`;

    const container = document.getElementById('workspaces-container');
    const errEl     = document.getElementById('dash-error');

    async function load() {
        try {
            const workspaces = await velcroApi('/workspaces');
            if (!workspaces) return;
            renderWorkspaces(workspaces);
        } catch (e) {
            showError(e.message);
        }
    }

    function renderWorkspaces(workspaces) {
        container.innerHTML = '';
        if (!workspaces.length) {
            container.innerHTML = '<p class="text-muted">Aucun workspace. Créez-en un !</p>';
            return;
        }
        workspaces.forEach(ws => {
            const col = document.createElement('div');
            col.className = 'col-md-4 mb-4';
            col.innerHTML = `
                <div class="card shadow-sm h-100">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <strong>${escHtml(ws.name)}</strong>
                        <button class="btn btn-sm btn-outline-danger btn-del-ws" data-id="${ws.id}" title="Supprimer">✕</button>
                    </div>
                    <div class="card-body boards-list" data-ws-id="${ws.id}">
                        <div class="text-muted small">Chargement…</div>
                    </div>
                    <div class="card-footer">
                        <button class="btn btn-sm btn-outline-primary w-100 btn-new-board" data-ws-id="${ws.id}">
                            + Nouveau board
                        </button>
                    </div>
                </div>`;
            container.appendChild(col);
            loadBoards(ws.id, col.querySelector('.boards-list'));
        });
    }

    async function loadBoards(wsId, el) {
        try {
            const boards = await velcroApi(`/boards/workspace/${wsId}`);
            el.innerHTML = '';
            if (!boards || !boards.length) {
                el.innerHTML = '<p class="text-muted small mb-0">Aucun board.</p>';
                return;
            }
            const group = document.createElement('div');
            group.className = 'list-group list-group-flush';
            boards.forEach(b => {
                const a = document.createElement('a');
                a.href = `/home/board/${b.id}`;
                a.className = 'list-group-item list-group-item-action py-2 ps-3';
                a.style.borderLeft = `5px solid ${b.backgroundColor || '#0079bf'}`;
                a.textContent = b.name;
                group.appendChild(a);
            });
            el.appendChild(group);
        } catch (e) {
            el.innerHTML = `<p class="text-danger small">${escHtml(e.message)}</p>`;
        }
    }

    function showError(msg) {
        errEl.textContent = msg;
        errEl.classList.remove('d-none');
    }

    // Create workspace
    document.getElementById('btn-new-workspace').addEventListener('click', async () => {
        const name = prompt('Nom du workspace :');
        if (!name || !name.trim()) return;
        try {
            await velcroApi('/workspaces', 'POST', { name: name.trim() });
            load();
        } catch (e) { showError(e.message); }
    });

    // Delegate: create board & delete workspace
    container.addEventListener('click', async e => {
        const btnBoard = e.target.closest('.btn-new-board');
        if (btnBoard) {
            const wsId = btnBoard.dataset.wsId;
            const name = prompt('Nom du board :');
            if (!name || !name.trim()) return;
            try {
                await velcroApi('/boards', 'POST', { workspaceId: wsId, name: name.trim() });
                load();
            } catch (e) { showError(e.message); }
        }

        const btnDel = e.target.closest('.btn-del-ws');
        if (btnDel) {
            if (!confirm('Supprimer ce workspace ?')) return;
            try {
                await velcroApi(`/workspaces/${btnDel.dataset.id}`, 'DELETE');
                load();
            } catch (e) { showError(e.message); }
        }
    });

    load();
});
