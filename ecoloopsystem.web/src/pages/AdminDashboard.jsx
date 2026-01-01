import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api';

function AdminDashboard() {
    const [rentals, setRentals] = useState([]);
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // 篩選狀態
    const [onlyUnreturned, setOnlyUnreturned] = useState(false);
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');

    const navigate = useNavigate();

    useEffect(() => {
        // 檢查是否已登入
        if (sessionStorage.getItem('adminLoggedIn') !== 'true') {
            navigate('/admin');
            return;
        }
        loadData();
    }, [navigate]);

    const loadData = async () => {
        setLoading(true);
        try {
            // 取得統計資訊
            const statsRes = await api.get('/admin/stats');
            setStats(statsRes.data);

            // 載入租借記錄
            await loadRentals();
        } catch (err) {
            console.error(err);
            setError('載入資料失敗');
        } finally {
            setLoading(false);
        }
    };

    const loadRentals = async () => {
        try {
            let url = '/admin/rentals?';
            if (onlyUnreturned) url += 'onlyUnreturned=true&';
            if (startDate) url += `startDate=${startDate}&`;
            if (endDate) url += `endDate=${endDate}&`;

            const res = await api.get(url);
            setRentals(res.data);
        } catch (err) {
            console.error(err);
            setError('載入租借記錄失敗');
        }
    };

    const handleFilter = async (e) => {
        e.preventDefault();
        await loadRentals();
    };

    const handleLogout = () => {
        sessionStorage.removeItem('adminLoggedIn');
        navigate('/admin');
    };

    const formatDate = (dateStr) => {
        if (!dateStr) return '-';
        return new Date(dateStr).toLocaleString('zh-TW');
    };

    if (loading) {
        return (
            <div className="card">
                <h1>📊 管理員儀表板</h1>
                <p>載入中...</p>
            </div>
        );
    }

    return (
        <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h1 style={{ margin: 0 }}>📊 管理員儀表板</h1>
                <button onClick={handleLogout} style={{ backgroundColor: '#dc3545' }}>
                    登出
                </button>
            </div>

            {error && (
                <div style={{
                    color: '#dc3545',
                    backgroundColor: '#f8d7da',
                    padding: '10px',
                    borderRadius: '4px',
                    marginBottom: '15px'
                }}>
                    {error}
                </div>
            )}

            {/* 統計卡片 */}
            {stats && (
                <div style={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
                    gap: '15px',
                    marginBottom: '30px'
                }}>
                    <div style={{ background: '#e3f2fd', padding: '20px', borderRadius: '8px', textAlign: 'center' }}>
                        <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1976d2' }}>{stats.totalRentals}</div>
                        <div style={{ color: '#666' }}>總租借次數</div>
                    </div>
                    <div style={{ background: '#fff3e0', padding: '20px', borderRadius: '8px', textAlign: 'center' }}>
                        <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#f57c00' }}>{stats.unreturnedCount}</div>
                        <div style={{ color: '#666' }}>未歸還</div>
                    </div>
                    <div style={{ background: '#e8f5e9', padding: '20px', borderRadius: '8px', textAlign: 'center' }}>
                        <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#388e3c' }}>{stats.totalUsers}</div>
                        <div style={{ color: '#666' }}>會員數</div>
                    </div>
                    <div style={{ background: '#f3e5f5', padding: '20px', borderRadius: '8px', textAlign: 'center' }}>
                        <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#7b1fa2' }}>{stats.totalTableware}</div>
                        <div style={{ color: '#666' }}>餐具數量</div>
                    </div>
                </div>
            )}

            {/* 篩選器 */}
            <form onSubmit={handleFilter} style={{
                background: '#f5f5f5',
                padding: '15px',
                borderRadius: '8px',
                marginBottom: '20px',
                display: 'flex',
                flexWrap: 'wrap',
                gap: '15px',
                alignItems: 'center'
            }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
                    <input
                        type="checkbox"
                        checked={onlyUnreturned}
                        onChange={(e) => setOnlyUnreturned(e.target.checked)}
                    />
                    只顯示未歸還
                </label>
                <label style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
                    開始日期:
                    <input
                        type="date"
                        value={startDate}
                        onChange={(e) => setStartDate(e.target.value)}
                        style={{ padding: '5px' }}
                    />
                </label>
                <label style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
                    結束日期:
                    <input
                        type="date"
                        value={endDate}
                        onChange={(e) => setEndDate(e.target.value)}
                        style={{ padding: '5px' }}
                    />
                </label>
                <button type="submit" style={{ padding: '8px 16px' }}>
                    🔍 篩選
                </button>
                <button type="button" onClick={() => { setOnlyUnreturned(false); setStartDate(''); setEndDate(''); }} style={{ padding: '8px 16px', backgroundColor: '#6c757d' }}>
                    清除篩選
                </button>
            </form>

            {/* 租借記錄表格 */}
            <div style={{ overflowX: 'auto' }}>
                <table style={{
                    width: '100%',
                    borderCollapse: 'collapse',
                    background: 'white',
                    boxShadow: '0 1px 3px rgba(0,0,0,0.1)'
                }}>
                    <thead>
                        <tr style={{ background: '#343a40', color: 'white' }}>
                            <th style={{ padding: '12px', textAlign: 'left' }}>ID</th>
                            <th style={{ padding: '12px', textAlign: 'left' }}>會員電話</th>
                            <th style={{ padding: '12px', textAlign: 'left' }}>卡片 UID</th>
                            <th style={{ padding: '12px', textAlign: 'left' }}>餐具 UID</th>
                            <th style={{ padding: '12px', textAlign: 'left' }}>類型</th>
                            <th style={{ padding: '12px', textAlign: 'left' }}>借用時間</th>
                            <th style={{ padding: '12px', textAlign: 'left' }}>歸還時間</th>
                            <th style={{ padding: '12px', textAlign: 'center' }}>狀態</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rentals.length === 0 ? (
                            <tr>
                                <td colSpan="8" style={{ padding: '20px', textAlign: 'center', color: '#666' }}>
                                    沒有租借記錄
                                </td>
                            </tr>
                        ) : (
                            rentals.map((rental) => (
                                <tr key={rental.id} style={{ borderBottom: '1px solid #dee2e6' }}>
                                    <td style={{ padding: '12px' }}>{rental.id}</td>
                                    <td style={{ padding: '12px' }}>{rental.userPhone}</td>
                                    <td style={{ padding: '12px', fontFamily: 'monospace' }}>{rental.userCardId}</td>
                                    <td style={{ padding: '12px', fontFamily: 'monospace' }}>{rental.tablewareTagId}</td>
                                    <td style={{ padding: '12px' }}>{rental.tablewareType}</td>
                                    <td style={{ padding: '12px' }}>{formatDate(rental.borrowedAt)}</td>
                                    <td style={{ padding: '12px' }}>{formatDate(rental.returnedAt)}</td>
                                    <td style={{ padding: '12px', textAlign: 'center' }}>
                                        {rental.isReturned ? (
                                            <span style={{ color: 'green' }}>✅ 已歸還</span>
                                        ) : (
                                            <span style={{ color: 'orange', fontWeight: 'bold' }}>⏳ 未歸還</span>
                                        )}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <p style={{ marginTop: '20px', textAlign: 'center', color: '#666' }}>
                共 {rentals.length} 筆記錄
            </p>
        </div>
    );
}

export default AdminDashboard;
