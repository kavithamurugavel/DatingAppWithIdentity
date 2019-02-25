import { Photo } from './photo';

// for explanation on types and interfaces, check section 9 lecture 80
export interface User {
    id: number;
    userName: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: Date;
    photoUrl: string;
    city: string;
    country: string;
    // following corresponds with UserForDetailedDTO from the API
    // ? is for optional properties, these properties should come after the reqd. properties
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
    roles?: string[];
}
