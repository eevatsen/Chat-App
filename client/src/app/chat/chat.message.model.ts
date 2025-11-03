export interface ChatMessage {
  id?: number; 
  user: string;
  text: string; 
  timesent?: Date;
  sentiment?: 'positive' | 'negative' | 'neutral' ;
}