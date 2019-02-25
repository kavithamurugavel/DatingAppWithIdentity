export interface Message {
    id: number;
    senderID: number;
    senderKnownAs: string;
    senderPhotoUrl: string;
    recipientID: number;
    recipientKnownAs: string;
    recipientPhotoUrl: string;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
}
