import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_client.dart';
import '../../../core/network/api_exception.dart';
import 'student_progress_response.dart';
import 'submit_quiz_answer_request.dart';

final quizApiProvider = Provider<QuizApi>((ref) {
  return QuizApi(ref.watch(apiClientProvider));
});

class QuizApi {
  QuizApi(this._dio);

  final Dio _dio;

  Future<StudentProgressResponse> submitAnswer(
    SubmitQuizAnswerRequest request,
  ) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/student-sessions/tasks/current/quiz-answer',
        data: request.toJson(),
      );

      final data = response.data;
      if (data == null) {
        throw const ApiException('Quiz cevap yanıtı boş geldi.');
      }

      return StudentProgressResponse.fromJson(data);
    } on DioException catch (exception) {
      throw mapDioException(exception);
    }
  }
}
