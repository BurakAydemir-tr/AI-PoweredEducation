import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../config/app_config.dart';
import '../storage/session_token_storage.dart';
import 'api_exception.dart';
import 'session_token_interceptor.dart';

final apiClientProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 15),
      sendTimeout: const Duration(seconds: 15),
      headers: {'Content-Type': 'application/json'},
    ),
  );

  dio.interceptors.add(
    SessionTokenInterceptor(ref.watch(sessionTokenStorageProvider)),
  );

  return dio;
});

ApiException mapDioException(DioException exception) {
  final responseData = exception.response?.data;
  final statusCode = exception.response?.statusCode;

  if (responseData is Map<String, dynamic>) {
    final message = responseData['detail'] ??
        responseData['message'] ??
        responseData['title'];

    if (message is String && message.isNotEmpty) {
      return ApiException(
        _translateApiMessage(message),
        statusCode: statusCode,
      );
    }
  }

  return ApiException(
    exception.message ?? 'İstek tamamlanamadı.',
    statusCode: statusCode,
  );
}

String _translateApiMessage(String message) {
  if (message.startsWith('BusinessRule.')) {
    return 'İşlem şu anda tamamlanamıyor. Lütfen bilgileri kontrol edip tekrar dene.';
  }

  return switch (message) {
    'Student name is already in use in this game.' =>
      'Bu oyunda bu öğrenci adı zaten kullanılıyor. Farklı bir ad deneyebilirsin.',
    'The student name is already in use in this game.' =>
      'Bu oyunda bu öğrenci adı zaten kullanılıyor. Farklı bir ad deneyebilirsin.',
    'The active game has no tasks.' => 'Aktif oyunda görev bulunmuyor.',
    'The session has no unfinished task.' =>
      'Bu oturumda tamamlanmamış görev bulunmuyor.',
    'The student session has ended.' => 'Öğrenci oturumu sona erdi.',
    'Incorrect answer. Try again.' => 'Cevap yanlış. Bir daha denemelisin.',
    'Incorrect answer. The correct answer is shown.' =>
      'Cevap yanlış. Doğru cevap gösterildi.',
    'Correct answer.' => 'Cevap doğru.',
    'Resource.NotFound' => 'Oyun bulunamadı. Oyun kodunu kontrol et.',
    _ => message,
  };
}
