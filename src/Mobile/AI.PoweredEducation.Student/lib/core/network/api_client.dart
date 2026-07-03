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
    final message = responseData['message'] ?? responseData['title'];

    if (message is String && message.isNotEmpty) {
      return ApiException(message, statusCode: statusCode);
    }
  }

  return ApiException(
    exception.message ?? 'The request could not be completed.',
    statusCode: statusCode,
  );
}
