import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

function Register() {
    const [cardId, setCardId] = useState('');
    const [phone, setPhone] = useState('');
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            await api.post('/users/register', { cardId, phoneNumber: phone });
            alert('註冊/綁定成功！');
            navigate(`/history/${phone}`);
        } catch (error) {
            console.error(error);
            alert('註冊失敗，請檢查輸入或稍後再試。');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="card">
            <h1>Ecoloop 會員綁定</h1>
            <p>請輸入您的悠遊卡號與手機號碼以查詢租借紀錄。</p>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>悠遊卡號 (Card ID)</label>
                    <input
                        type="text"
                        value={cardId}
                        onChange={(e) => setCardId(e.target.value)}
                        placeholder="請輸入卡片內碼 (例如: 1A2B3C4D)"
                        required
                    />
                </div>
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
                <button type="submit" disabled={loading}>
                    {loading ? '處理中...' : '綁定並查詢'}
                </button>
            </form>
        </div>
    );
}

export default Register;
