import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import signalRService from '../services/SignalRService';
import { List, Button, message, Modal, Input } from 'antd';
import { useNavigate } from 'react-router-dom';

const LobbyPage = () => {
    const { token, logout } = useAuth();
    const navigate = useNavigate();
    const [rooms, setRooms] = useState([]);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [newRoomName, setNewRoomName] = useState('');

    useEffect(() => {
        if (token) {
            signalRService.startConnection(token);

            signalRService.on('ReceiveRoomList', (roomList) => {
                setRooms(roomList);
            });

            signalRService.on('RoomCreated', (room) => {
                setRooms((prevRooms) => [...prevRooms, room]);
            });
            
            signalRService.on('RoomJoined', (roomId) => {
                navigate(`/game/${roomId}`);
            });
            
            // Request initial room list
            signalRService.invoke('GetRooms');
        }

        return () => {
            signalRService.off('ReceiveRoomList');
            signalRService.off('RoomCreated');
            signalRService.off('RoomJoined');
            signalRService.stopConnection();
        };
    }, [token, navigate]);

    const createRoom = async () => {
        if (!newRoomName) return;
        try {
            await signalRService.invoke('CreateRoom', newRoomName);
            setIsModalVisible(false);
            setNewRoomName('');
            message.success('Room created successfully');
        } catch (error) {
            message.error('Failed to create room');
        }
    };
    
    const joinRoom = async (roomId) => {
        try {
             await signalRService.invoke('JoinRoom', roomId);
        } catch (error) {
            message.error('Failed to join room');
        }
    }

    return (
        <div style={{ padding: '20px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '20px' }}>
                <h1>Game Lobby</h1>
                <div>
                    <Button type="primary" onClick={() => setIsModalVisible(true)} style={{ marginRight: '10px' }}>
                        Create Room
                    </Button>
                    <Button onClick={logout}>Logout</Button>
                </div>
            </div>

            <List
                grid={{ gutter: 16, column: 4 }}
                dataSource={rooms}
                renderItem={(item) => (
                    <List.Item>
                        <Button type="default" block onClick={() => joinRoom(item.id)}>
                            {item.name} ({item.players ? item.players.length : 0}/3)
                        </Button>
                    </List.Item>
                )}
            />

            <Modal
                title="Create Room"
                open={isModalVisible}
                onOk={createRoom}
                onCancel={() => setIsModalVisible(false)}
            >
                <Input
                    placeholder="Room Name"
                    value={newRoomName}
                    onChange={(e) => setNewRoomName(e.target.value)}
                />
            </Modal>
        </div>
    );
};

export default LobbyPage;
