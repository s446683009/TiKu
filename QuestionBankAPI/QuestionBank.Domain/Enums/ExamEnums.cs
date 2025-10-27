namespace QuestionBank.Domain.Enums;

public enum ExamStatus
{
    Draft = 0,          // 草稿
    Published = 1,      // 已发布
    InProgress = 2,     // 进行中
    Ended = 3,          // 已结束
    Cancelled = 4       // 已取消
}

public enum AnswerDisplayMode
{
    AfterSubmit = 1,        // 交卷后显示
    AfterExamEnd = 2,       // 考试结束后显示
    Never = 3               // 永不显示
}
