import * as signalR from '@microsoft/signalr';

class SignalRService {
    constructor() {
        this.connection = null;
    }

    startConnection = async (token) => {
        if (!token) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/gameHub', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.connection.onclose(async () => {
            console.log('SignalR connection closed.');
            // Optionally try to reconnect
        });

        try {
            await this.connection.start();
            console.log('SignalR Connected.');
        } catch (err) {
            console.log('Error while establishing connection: ' + err);
            setTimeout(() => this.startConnection(token), 5000);
        }
    };

    on = (methodName, newMethod) => {
        if (this.connection) {
            this.connection.on(methodName, newMethod);
        }
    };

    off = (methodName, method) => {
        if (this.connection) {
            this.connection.off(methodName, method);
        }
    };

    invoke = async (methodName, ...args) => {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            return await this.connection.invoke(methodName, ...args);
        } else {
            console.error('SignalR connection is not in the Connected state.');
        }
    };

    stopConnection = async () => {
        if (this.connection) {
            await this.connection.stop();
            console.log('SignalR Disconnected.');
        }
    };
}

const signalRService = new SignalRService();
export default signalRService;
