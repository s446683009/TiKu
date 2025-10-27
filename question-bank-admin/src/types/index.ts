// 用户角色常量
export const UserRole = {
  Student: 1,
  Teacher: 2,
  Admin: 3
} as const;

export type UserRole = typeof UserRole[keyof typeof UserRole];

// 题目类型常量
export const QuestionType = {
  SingleChoice: 1,
  MultipleChoice: 2,
  TrueFalse: 3,
  FillBlank: 4,
  ShortAnswer: 5,
  Material: 6
} as const;

export type QuestionType = typeof QuestionType[keyof typeof QuestionType];

// 难度等级常量
export const Difficulty = {
  VeryEasy: 1,
  Easy: 2,
  Medium: 3,
  Hard: 4,
  VeryHard: 5
} as const;

export type Difficulty = typeof Difficulty[keyof typeof Difficulty];

// 用户类型
export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
}

// 登录响应
export interface LoginResponse {
  token: string;
  user: User;
}

// API 响应
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

// 分页响应
export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// 知识点
export interface KnowledgePoint {
  id: string;
  name: string;
  description?: string;
  level: number;
  parentId?: string;
  createdAt: string;
}

// 题目
export interface Question {
  id: string;
  type: QuestionType;
  content: string;
  options?: string;
  correctAnswer: string;
  explanation?: string;
  difficulty: Difficulty;
  score: number;
  chapter?: string;
  status: number;
  knowledgePoints: KnowledgePoint[];
  createdAt: string;
}

// 试卷
export interface Paper {
  id: string;
  title: string;
  description?: string;
  totalScore: number;
  duration: number;
  questionCount: number;
  createdAt: string;
}

// 考试
export interface Exam {
  id: string;
  title: string;
  description?: string;
  paperId: string;
  status: number;
  startTime?: string;
  endTime?: string;
  duration: number;
  maxAttempts: number;
  createdAt: string;
}

// 错题记录
export interface WrongQuestion {
  id: string;
  questionId: string;
  wrongCount: number;
  lastWrongAt: string;
  question: Question;
}

// 收藏
export interface FavoriteQuestion {
  id: string;
  questionId: string;
  note?: string;
  createdAt: string;
  question: Question;
}

// 笔记
export interface QuestionNote {
  id: string;
  questionId: string;
  content: string;
  createdAt: string;
  updatedAt: string;
  question?: Question;
}
