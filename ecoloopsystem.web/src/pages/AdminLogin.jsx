import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

function AdminLogin() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await api.post('/admin/login', {
                username: username,
                password: password
            });

            if (response.data.success) {
                // 儲存登入狀態
                sessionStorage.setItem('adminLoggedIn', 'true');
                navigate('/admin/dashboard');
            }
        } catch (err) {
            console.error(err);
            const message = err.response?.data?.message || '登入失敗，請重試。';
            setError(message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="card">
            <h1>🔐 管理員登入</h1>
            <p>請輸入管理員帳號密碼。</p>

            {error && (
                <div className="error-message" style={{
                    color: '#dc3545',
                    backgroundColor: '#f8d7da',
                    padding: '10px',
                    borderRadius: '4px',
                    marginBottom: '15px'
                }}>
                    {error}
                </div>
            )}

            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>帳號</label>
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        placeholder="admin"
                        required
                    />
                </div>
                <div className="form-group">
                    <label>密碼</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        placeholder="請輸入密碼"
                        required
                    />
                </div>
                <button type="submit" disabled={loading}>
                    {loading ? '登入中...' : '登入'}
                </button>
            </form>

            <p style={{ marginTop: '20px', fontSize: '14px', color: '#666' }}>
                <a href="/" style={{ color: '#007bff' }}>← 返回會員登入</a>
            </p>
        </div>
    );
}

export default AdminLogin;
