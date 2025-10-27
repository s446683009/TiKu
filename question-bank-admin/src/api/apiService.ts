import axios from 'axios';
import type { AxiosInstance, AxiosRequestConfig } from 'axios';
import type { ApiResponse, LoginResponse, User } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

class ApiService {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // 请求拦截器 - 添加 token
    this.client.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // 响应拦截器 - 处理错误
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token 过期，清除并跳转到登录页
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // 通用请求方法
  private async request<T>(config: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await this.client.request<ApiResponse<T>>(config);
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || error.message || '请求失败',
        errors: error.response?.data?.errors
      };
    }
  }

  // 认证相关
  async login(username: string, password: string) {
    return this.request<LoginResponse>({
      method: 'POST',
      url: '/auth/login',
      data: { username, password }
    });
  }

  async register(data: {
    username: string;
    password: string;
    email: string;
    fullName: string;
    phone?: string;
  }) {
    return this.request<User>({
      method: 'POST',
      url: '/auth/register',
      data
    });
  }

  // 用户相关
  async getCurrentUser() {
    return this.request<User>({
      method: 'GET',
      url: '/users/me'
    });
  }

  // 题目相关
  async searchQuestions(params: {
    keyword?: string;
    type?: number;
    difficulty?: number;
    pageNumber?: number;
    pageSize?: number;
  }) {
    return this.request({
      method: 'POST',
      url: '/questions/search',
      data: params
    });
  }

  async getQuestion(id: string) {
    return this.request({
      method: 'GET',
      url: `/questions/${id}`
    });
  }

  async createQuestion(data: any) {
    return this.request({
      method: 'POST',
      url: '/questions',
      data
    });
  }

  async updateQuestion(id: string, data: any) {
    return this.request({
      method: 'PUT',
      url: `/questions/${id}`,
      data
    });
  }

  async deleteQuestion(id: string) {
    return this.request({
      method: 'DELETE',
      url: `/questions/${id}`
    });
  }

  // 试卷相关
  async getPapers(pageNumber = 1, pageSize = 20) {
    return this.request({
      method: 'GET',
      url: `/papers?pageNumber=${pageNumber}&pageSize=${pageSize}`
    });
  }

  async getPaperDetail(id: string) {
    return this.request({
      method: 'GET',
      url: `/papers/${id}/detail`
    });
  }

  async createPaper(data: any) {
    return this.request({
      method: 'POST',
      url: '/papers',
      data
    });
  }

  async updatePaper(id: string, data: any) {
    return this.request({
      method: 'PUT',
      url: `/papers/${id}`,
      data
    });
  }

  async deletePaper(id: string) {
    return this.request({
      method: 'DELETE',
      url: `/papers/${id}`
    });
  }

  // 考试相关
  async getExams() {
    return this.request({
      method: 'GET',
      url: '/exams'
    });
  }

  async createExam(data: any) {
    return this.request({
      method: 'POST',
      url: '/exams',
      data
    });
  }

  // 知识点相关
  async getKnowledgePoints() {
    return this.request({
      method: 'GET',
      url: '/knowledgepoints'
    });
  }

  async createKnowledgePoint(data: { name: string; description?: string; parentId?: string }) {
    return this.request({
      method: 'POST',
      url: '/knowledgepoints',
      data
    });
  }

  // 学习功能相关
  async getWrongQuestions(pageNumber = 1, pageSize = 20) {
    return this.request({
      method: 'GET',
      url: `/learning/wrong-questions?pageNumber=${pageNumber}&pageSize=${pageSize}`
    });
  }

  async getFavorites(pageNumber = 1, pageSize = 20) {
    return this.request({
      method: 'GET',
      url: `/learning/favorites?pageNumber=${pageNumber}&pageSize=${pageSize}`
    });
  }

  async getNotes(pageNumber = 1, pageSize = 20) {
    return this.request({
      method: 'GET',
      url: `/learning/notes?pageNumber=${pageNumber}&pageSize=${pageSize}`
    });
  }

  async addToWrongQuestions(questionId: string) {
    return this.request({
      method: 'POST',
      url: '/learning/wrong-questions',
      data: { questionId }
    });
  }

  async addToFavorites(questionId: string, note?: string) {
    return this.request({
      method: 'POST',
      url: '/learning/favorites',
      data: { questionId, note }
    });
  }

  async createNote(questionId: string, content: string) {
    return this.request({
      method: 'POST',
      url: '/learning/notes',
      data: { questionId, content }
    });
  }
}

export const apiService = new ApiService();
export default apiService;
