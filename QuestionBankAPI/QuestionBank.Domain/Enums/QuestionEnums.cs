namespace QuestionBank.Domain.Enums;

public enum QuestionType
{
    SingleChoice = 1,   // 单选题
    MultipleChoice = 2, // 多选题
    TrueFalse = 3,      // 判断题
    FillBlank = 4,      // 填空题
    ShortAnswer = 5,    // 简答题
    Material = 6        // 材料题(一个材料下多个小题)
}

public enum DifficultyLevel
{
    VeryEasy = 1,
    Easy = 2,
    Medium = 3,
    Hard = 4,
    VeryHard = 5
}

public enum QuestionStatus
{
    Disabled = 0,   // 禁用
    Enabled = 1     // 启用
}
