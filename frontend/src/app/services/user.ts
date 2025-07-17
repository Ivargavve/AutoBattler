import { Character } from '../services/character';

export interface User {
  id: number;
  username: string;
  role: string;

  fullName: string;
  profilePictureUrl: string;
  googleId: string;

  needsUsernameSetup: boolean;

  experiencePoints: number;
  maxExperiencePoints: number;
  level: number;
  credits: number;

  cosmeticItemsJson: string;
  settingsJson: string;
  achievementsJson: string;

  createdAt: string;
  lastLogin: string;

  character?: Character;
}
