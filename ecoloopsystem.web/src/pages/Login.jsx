import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

function Login() {
    const [phone, setPhone] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await api.post('/users/login', {
                phoneNumber: phone,
                password: password
            });

            if (response.data.success) {
                navigate(`/history/${phone}`);
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
            <h1>Ecoloop 會員登入</h1>
            <p>請輸入手機號碼和密碼以查詢租借紀錄。</p>

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
                    <label>手機號碼</label>
                    <input
                        type="tel"
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        placeholder="0912345678"
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
                尚未註冊？請前往服務據點進行卡片註冊。
            </p>
        </div>
    );
}

export default Login;
