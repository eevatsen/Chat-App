// client/src/app/chat.service.ts
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

  // Використовуємо BehaviorSubject для зберігання повідомлень
  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/chatHub') // Той самий шлях, що й у proxy.conf.json
      .withAutomaticReconnect()
      .build();
  }

  /** Запускає підключення до хабу */
  public async startConnection() {
    try {
      await this.hubConnection.start();
      console.log('SignalR Connection started');
      this.registerReceiveHandler();
      this.loadHistory(); // Завантажуємо історію після підключення
    } catch (err) {
      console.error('Error while starting SignalR connection: ' + err);
      // Спробуємо перепідключитись через 5 секунд
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  /** Зупиняє підключення */
  public stopConnection() {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.stop();
    }
  }

  /** Відправляє повідомлення на сервер */
  public async sendMessage(user: string, message: string) {
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error('Hub not connected');
      return;
    }

    try {
      // Назва методу "SendMessage" має співпадати з назвою методу в ChatHub.cs
      await this.hubConnection.invoke('SendMessage', user, message);
    } catch (err) {
      console.error('Error while sending message: ' + err);
    }
  }

  /** Реєструє слухача для вхідних повідомлень */
  private registerReceiveHandler() {
    // "ReceiveMessage" - назва, яку бекенд використовує в `Clients.All.SendAsync(...)`
    this.hubConnection.on(
      'ReceiveMessage',
      (user: string, message: string, sentiment?: string) => {
        const fullMessage: ChatMessage = {
          user,
          text: message,
          //sentiment: sentiment?.toLowerCase() as any,
          timesent: new Date(),
        };

        // Додаємо нове повідомлення до поточного списку
        const currentMessages = this.messagesSubject.value;
        this.messagesSubject.next([...currentMessages, fullMessage]);
      }
    );
  }

  /** Завантажує історію чату з API */
  private loadHistory() {
    // Потрібен API-ендпоінт на бекенді, наприклад /api/messages
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