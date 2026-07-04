import '../../../core/models/quiz_answer_option.dart';
import '../../../core/models/student_task_response.dart';

class StudentProgressResponse {
  const StudentProgressResponse({
    required this.studentSessionId,
    this.currentTask,
    this.result,
    this.feedback,
    this.revealedCorrectAnswer,
  });

  final String studentSessionId;
  final StudentTaskResponse? currentTask;
  final Map<String, dynamic>? result;
  final String? feedback;
  final QuizAnswerOption? revealedCorrectAnswer;

  factory StudentProgressResponse.fromJson(Map<String, dynamic> json) {
    final currentTaskJson = json['currentTask'];
    final revealedCorrectAnswerJson = json['revealedCorrectAnswer'];

    return StudentProgressResponse(
      studentSessionId: json['studentSessionId'] as String,
      currentTask: currentTaskJson is Map<String, dynamic>
          ? StudentTaskResponse.fromJson(currentTaskJson)
          : null,
      result: json['result'] as Map<String, dynamic>?,
      feedback: json['feedback'] as String?,
      revealedCorrectAnswer: revealedCorrectAnswerJson is int
          ? QuizAnswerOption.fromJson(revealedCorrectAnswerJson)
          : null,
    );
  }
}
