const TOKEN_KEY = 'velcro_token';
const USER_KEY  = 'velcro_user';

function getToken() { return localStorage.getItem(TOKEN_KEY); }
function getUser()  { return JSON.parse(localStorage.getItem(USER_KEY) || 'null'); }

function saveAuth(accessToken, user) {
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
}

function clearAuth() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
}

function requireAuth() {
    if (!getToken()) {
        window.location.href = '/account/login';
        return false;
    }
    return true;
}

async function velcroApi(path, method = 'GET', body = null) {
    const opts = {
        method,
        headers: { 'Content-Type': 'application/json' }
    };
    const token = getToken();
    if (token) opts.headers['Authorization'] = `Bearer ${token}`;
    if (body !== null) opts.body = JSON.stringify(body);

    const res = await fetch('/api' + path, opts);

    if (res.status === 401) {
        clearAuth();
        window.location.href = '/account/login';
        return null;
    }
    if (res.status === 204 || res.status === 200 && res.headers.get('content-length') === '0') return null;
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `Erreur ${res.status}`);
    }
    // 204 has no body
    const ct = res.headers.get('content-type') || '';
    if (!ct.includes('json')) return null;
    return res.json();
}

function escHtml(str) {
    if (!str) return '';
    return String(str).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

function formatDate(iso) {
    if (!iso) return '';
    return new Date(iso).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short' });
}
