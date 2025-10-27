-- 创建数据库
-- CREATE DATABASE questionbank;

-- 初始化用户数据 (管理员账户)
-- 密码为: Admin123! (已使用BCrypt加密)

-- 注意: 实际使用时,请先运行 EF Core 迁移创建表结构
-- dotnet ef migrations add InitialCreate --startup-project ../QuestionBank.API
-- dotnet ef database update --startup-project ../QuestionBank.API

-- 然后可以运行以下SQL插入初始数据

-- 插入管理员用户
INSERT INTO users (id, username, password_hash, email, full_name, role, is_active, created_at, updated_at, is_deleted)
VALUES
('00000000-0000-0000-0000-000000000001',
 'admin',
 '$2a$11$8K1p/a0dL3LlL/OowFIhiOv/WA6q8l.SqG9TnK5eJFVBH1LJmBRLu',
 'admin@questionbank.com',
 '系统管理员',
 3,
 true,
 NOW(),
 NULL,
 false);

-- 插入测试教师用户 (密码: Teacher123!)
INSERT INTO users (id, username, password_hash, email, full_name, role, is_active, created_at, updated_at, is_deleted)
VALUES
('00000000-0000-0000-0000-000000000002',
 'teacher1',
 '$2a$11$8K1p/a0dL3LlL/OowFIhiOv/WA6q8l.SqG9TnK5eJFVBH1LJmBRLu',
 'teacher1@questionbank.com',
 '张老师',
 2,
 true,
 NOW(),
 NULL,
 false);

-- 插入测试学生用户 (密码: Student123!)
INSERT INTO users (id, username, password_hash, email, full_name, role, is_active, created_at, updated_at, is_deleted)
VALUES
('00000000-0000-0000-0000-000000000003',
 'student1',
 '$2a$11$8K1p/a0dL3LlL/OowFIhiOv/WA6q8l.SqG9TnK5eJFVBH1LJmBRLu',
 'student1@questionbank.com',
 '李同学',
 1,
 true,
 NOW(),
 NULL,
 false);

-- 插入知识点示例数据
INSERT INTO knowledge_points (id, name, description, parent_id, level, sort_order, created_at, is_deleted)
VALUES
('10000000-0000-0000-0000-000000000001', 'C#编程', 'C#编程语言基础与高级', NULL, 1, 1, NOW(), false),
('10000000-0000-0000-0000-000000000002', '基础语法', 'C#基础语法', '10000000-0000-0000-0000-000000000001', 2, 1, NOW(), false),
('10000000-0000-0000-0000-000000000003', '面向对象', 'C#面向对象编程', '10000000-0000-0000-0000-000000000001', 2, 2, NOW(), false),
('10000000-0000-0000-0000-000000000004', '数据库', '数据库原理与应用', NULL, 1, 2, NOW(), false),
('10000000-0000-0000-0000-000000000005', 'SQL基础', 'SQL语句基础', '10000000-0000-0000-0000-000000000004', 2, 1, NOW(), false);

-- 插入题目示例数据
INSERT INTO questions (id, type, content, options, correct_answer, explanation, difficulty, score, chapter, status, creator_id, created_at, is_deleted)
VALUES
('20000000-0000-0000-0000-000000000001',
 1, -- SingleChoice
 'C#中,以下哪个关键字用于声明一个类?',
 '["A. class", "B. struct", "C. interface", "D. enum"]',
 'A',
 'class关键字用于声明一个类',
 1, -- VeryEasy
 2,
 '第一章',
 1, -- Enabled
 '00000000-0000-0000-0000-000000000002',
 NOW(),
 false);

INSERT INTO questions (id, type, content, options, correct_answer, explanation, difficulty, score, chapter, status, creator_id, created_at, is_deleted)
VALUES
('20000000-0000-0000-0000-000000000002',
 2, -- MultipleChoice
 'C#中,以下哪些是值类型?',
 '["A. int", "B. string", "C. double", "D. bool"]',
 'A,C,D',
 'int、double和bool是值类型,string是引用类型',
 3, -- Medium
 5,
 '第一章',
 1,
 '00000000-0000-0000-0000-000000000002',
 NOW(),
 false);

INSERT INTO questions (id, type, content, options, correct_answer, explanation, difficulty, score, chapter, status, creator_id, created_at, is_deleted)
VALUES
('20000000-0000-0000-0000-000000000003',
 3, -- TrueFalse
 'C#是一种面向对象的编程语言。',
 '["A. 正确", "B. 错误"]',
 'A',
 'C#确实是一种面向对象的编程语言',
 1,
 2,
 '第一章',
 1,
 '00000000-0000-0000-0000-000000000002',
 NOW(),
 false);

-- 关联题目和知识点
INSERT INTO question_knowledge_points (question_id, knowledge_point_id)
VALUES
('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002'),
('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002'),
('20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003');

-- 查询验证
-- SELECT * FROM users;
-- SELECT * FROM knowledge_points ORDER BY level, sort_order;
-- SELECT * FROM questions;
