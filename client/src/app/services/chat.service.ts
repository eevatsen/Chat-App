import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, tap } from 'rxjs';
import { ChatMessage } from '../chat/chat.message.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private http = inject(HttpClient);
  private hubConnection: signalR.HubConnection;

  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/chatHub') 
      .withAutomaticReconnect()
      .build();
  }

 
  public async startConnection() {
    try {
      await this.hubConnection.start();
      console.log('SignalR Connection started');
      this.registerReceiveHandler();
      this.loadHistory(); 
    } catch (err) {
      console.error('Error while starting SignalR connection: ' + err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  public stopConnection() {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.stop();
    }
  }

  public async sendMessage(user: string, message: string) {
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error('Hub not connected');
      return;
    }

    try {
      await this.hubConnection.invoke('SendMessage', user, message);
    } catch (err) {
      console.error('Error while sending message: ' + err);
    }
  }

  private registerReceiveHandler() {
    this.hubConnection.on(
      'ReceiveMessage',
      (user: string, message: string, sentiment?: string) => {
        const fullMessage: ChatMessage = {
          user,
          text: message,
          sentiment: sentiment?.toLowerCase() as any,
          timesent: new Date(),
        };

        const currentMessages = this.messagesSubject.value;
        this.messagesSubject.next([...currentMessages, fullMessage]);
      }
    );
  }

  private loadHistory() {
    this.http.get<ChatMessage[]>('/api/messages')
      .pipe(
        tap(messages => console.log(`Loaded ${messages.length} messages from history`))
      )
      .subscribe({
        next: (messages) => this.messagesSubject.next(messages),
        error: (err) => console.error('Could not load chat history:', err)
      });
  }
}