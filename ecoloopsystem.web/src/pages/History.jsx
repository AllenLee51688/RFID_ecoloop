import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../api';

function History() {
    const { phone } = useParams();
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchHistory = async () => {
            try {
                const response = await api.get(`/users/${phone}/history`);
                setHistory(response.data);
            } catch (error) {
                console.error(error);
                alert('無法取得歷史紀錄');
            } finally {
                setLoading(false);
            }
        };

        fetchHistory();
    }, [phone]);

    return (
        <div className="card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h1>租借紀錄</h1>
                <Link to="/" style={{ color: '#2ecc71' }}>返回首頁</Link>
            </div>
            <p>手機號碼: {phone}</p>

            {loading ? (
                <p>載入中...</p>
            ) : history.length === 0 ? (
                <p>目前沒有租借紀錄。</p>
            ) : (
                <div>
                    {history.map((item) => (
                        <div key={item.id} className="history-item">
                            <div>
                                <strong>{item.tablewareType}</strong>
                                <br />
                                <small style={{ color: '#7f8c8d' }}>
                                    借出: {new Date(item.borrowedAt).toLocaleString()}
                                </small>
                            </div>
                            <div className={item.returnedAt ? 'status-returned' : 'status-rented'}>
                                {item.returnedAt ? (
                                    <span>已歸還<br /><small>{new Date(item.returnedAt).toLocaleString()}</small></span>
                                ) : (
                                    '租借中'
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default History;
