import { Component, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { Router } from '@angular/router';
import { AiService, ChatMessage, PostProposal } from '../../services/ai.service';
import { AuthService } from '../../auth/services/auth.service';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-ai-chat',
  standalone: false,
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.css'
})
export class AiChatComponent implements AfterViewChecked {
  @ViewChild('messageList') private messageList!: ElementRef;

  messages: ChatMessage[] = [];
  inputText = '';
  isLoading = false;
  isGenerating = false;
  isCreating = false;
  postProposal: PostProposal | null = null;
  showChangePassword = false;
  showProfileMenu = false;
  private shouldScrollToBottom = false;

  constructor(
    private aiService: AiService,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  get hasAiResponse(): boolean {
    return this.messages.some(m => m.role === 'assistant');
  }

  sendMessage(): void {
    const content = this.inputText.trim();
    if (!content || this.isLoading) return;

    this.messages.push({ role: 'user', content });
    this.inputText = '';
    this.isLoading = true;
    this.shouldScrollToBottom = true;

    this.aiService.chat(this.messages).subscribe({
      next: (response) => {
        this.messages.push({ role: 'assistant', content: response.message });
        this.isLoading = false;
        this.shouldScrollToBottom = true;
      },
      error: () => {
        this.isLoading = false;
        this.notificationService.showError('Failed to get a response. Please try again.');
      }
    });
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  generatePost(): void {
    this.isGenerating = true;
    this.postProposal = null;

    this.aiService.generatePost(this.messages).subscribe({
      next: (proposal) => {
        this.postProposal = proposal;
        this.isGenerating = false;
      },
      error: () => {
        this.isGenerating = false;
        this.notificationService.showError('Failed to generate post. Please try again.');
      }
    });
  }

  createPost(): void {
    if (!this.postProposal) return;

    this.isCreating = true;
    this.aiService.createPost(this.postProposal).subscribe({
      next: (result) => {
        this.isCreating = false;
        this.notificationService.showSuccess('Post created successfully!');
        this.router.navigate(['/editor/edit', result.postId]);
      },
      error: () => {
        this.isCreating = false;
        this.notificationService.showError('Failed to create post. Please try again.');
      }
    });
  }

  discardProposal(): void {
    this.postProposal = null;
  }

  private scrollToBottom(): void {
    try {
      this.messageList.nativeElement.scrollTop = this.messageList.nativeElement.scrollHeight;
    } catch { }
  }

  logout(): void {
    this.authService.signOut();
    this.router.navigate(['/auth/login']);
  }

  toggleProfileMenu(): void {
    this.showProfileMenu = !this.showProfileMenu;
  }

  openChangePassword(): void {
    this.showProfileMenu = false;
    this.showChangePassword = true;
  }

  closeChangePassword(): void {
    this.showChangePassword = false;
  }
}
