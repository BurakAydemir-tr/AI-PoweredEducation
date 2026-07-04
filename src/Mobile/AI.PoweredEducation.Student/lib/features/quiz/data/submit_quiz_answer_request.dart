import '../../../core/models/quiz_answer_option.dart';

class SubmitQuizAnswerRequest {
  const SubmitQuizAnswerRequest({required this.answer});

  final QuizAnswerOption answer;

  Map<String, dynamic> toJson() {
    return {'answer': answer.value};
  }
}
