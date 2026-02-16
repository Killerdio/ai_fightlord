import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import signalRService from '../services/SignalRService';
import { Button, message, Card } from 'antd';

const GamePage = () => {
    const { id } = useParams();
    const { token, user } = useAuth();
    const navigate = useNavigate();
    const [gameState, setGameState] = useState(null);
    const [selectedCards, setSelectedCards] = useState([]);

    useEffect(() => {
        if (token) {
            signalRService.startConnection(token);

            signalRService.on('GameStateUpdate', (state) => {
                setGameState(state);
            });

            signalRService.on('GameEnded', (winner) => {
                message.info(`Game over! Winner: ${winner}`);
                navigate('/lobby');
            });

            // Initial state request
            signalRService.invoke('GetGameState', id);
        }

        return () => {
            signalRService.off('GameStateUpdate');
            signalRService.off('GameEnded');
            signalRService.stopConnection();
        };
    }, [id, token, navigate]);

    const playCards = async () => {
        if (selectedCards.length === 0) return;
        try {
            await signalRService.invoke('PlayCards', id, selectedCards);
            setSelectedCards([]);
        } catch (error) {
            message.error('Failed to play cards');
        }
    };

    const passTurn = async () => {
        try {
            await signalRService.invoke('PassTurn', id);
        } catch (error) {
            message.error('Failed to pass turn');
        }
    };

    if (!gameState) {
        return <div>Loading game...</div>;
    }

    return (
        <div style={{ padding: '20px' }}>
            <h1>Game Room: {id}</h1>
            <div>
                <h2>Current Player: {gameState.currentPlayer}</h2>
                <div>
                    <h3>Your Hand:</h3>
                    {gameState.hand && gameState.hand.map((card) => (
                        <Card
                            key={card.id}
                            style={{ width: 100, display: 'inline-block', margin: '5px', border: selectedCards.includes(card.id) ? '2px solid blue' : '1px solid gray' }}
                            onClick={() => {
                                if (selectedCards.includes(card.id)) {
                                    setSelectedCards(selectedCards.filter(c => c !== card.id));
                                } else {
                                    setSelectedCards([...selectedCards, card.id]);
                                }
                            }}
                        >
                            {card.rank}{card.suit}
                        </Card>
                    ))}
                </div>
                <div style={{ marginTop: '20px' }}>
                    <Button type="primary" onClick={playCards} disabled={gameState.currentPlayer !== user.username}>
                        Play
                    </Button>
                    <Button onClick={passTurn} disabled={gameState.currentPlayer !== user.username} style={{ marginLeft: '10px' }}>
                        Pass
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default GamePage;
