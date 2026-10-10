import 'package:ai_powered_education_student/core/models/quiz_answer_option.dart';
import 'package:ai_powered_education_student/features/quiz/data/submit_quiz_answer_request.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('quiz answer includes task identity and expected attempt count', () {
    const request = SubmitQuizAnswerRequest(
      taskId: '12345678-1234-1234-1234-123456789012',
      expectedAttemptCount: 2,
      answer: QuizAnswerOption.optionB,
    );

    expect(request.toJson(), {
      'taskId': '12345678-1234-1234-1234-123456789012',
      'expectedAttemptCount': 2,
      'answer': 1,
    });
  });
}
