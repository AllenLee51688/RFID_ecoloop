import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import History from './pages/History';
import './App.css';

function App() {
  return (
    <Router>
      <div className="container">
        <Routes>
          <Route path="/" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/history/:phone" element={<History />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
