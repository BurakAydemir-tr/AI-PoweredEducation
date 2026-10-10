import '../../../core/models/quiz_answer_option.dart';

class SubmitQuizAnswerRequest {
  const SubmitQuizAnswerRequest({
    required this.taskId,
    required this.expectedAttemptCount,
    required this.answer,
  });

  final String taskId;
  final int expectedAttemptCount;
  final QuizAnswerOption answer;

  Map<String, dynamic> toJson() {
    return {
      'taskId': taskId,
      'expectedAttemptCount': expectedAttemptCount,
      'answer': answer.value,
    };
  }
}
