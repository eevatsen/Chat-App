import { Component, OnInit, OnDestroy, inject, signal, effect, ViewChild, ElementRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../services/chat.service';
import { ChatMessage } from '../chat/chat.message.model';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
  changeDetection: ChangeDetectionStrategy.OnPush, 
})
export class ChatComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);

  @ViewChild('messageContainer')
  private messageContainer!: ElementRef;

  public messages$: Observable<ChatMessage[]> = this.chatService.messages$;
  
  public sUser = signal<string>('');
  public sMessage = signal<string>('');

  constructor() {
    effect(() => {
      this.messages$.subscribe(() => {
        this.scrollToBottom();
      });
    });
  }

  ngOnInit() {
    this.chatService.startConnection();
    
    const savedUser = localStorage.getItem('chatUser');
    if (savedUser) {
      this.sUser.set(savedUser);
    } else {
      const newUser = prompt('Enter your name:');
      if (newUser) {
        this.sUser.set(newUser);
        localStorage.setItem('chatUser', newUser);
      }
    }
  }

  ngOnDestroy() {
    this.chatService.stopConnection(); 
  }

  sendMessage() {
    const user = this.sUser();
    const message = this.sMessage();
    if (user && message) {
      this.chatService.sendMessage(user, message);
      this.sMessage.set(''); 
    }
  }

  getSentimentEmoji(sentiment?: string): string {
    switch (sentiment) {
      case 'positive':
        return '😊';
      case 'negative':
        return '😠';
      case 'neutral':
        return '😐';
      default:
        return '';
    }
  }

  private scrollToBottom(): void {
    try {
      setTimeout(() => {
        this.messageContainer.nativeElement.scrollTop = this.messageContainer.nativeElement.scrollHeight;
      }, 50); 
    } catch(err) { }
  }
}