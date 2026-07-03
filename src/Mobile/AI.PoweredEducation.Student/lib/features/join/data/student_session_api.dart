import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_client.dart';
import '../../../core/network/api_exception.dart';
import '../../../core/storage/session_token_storage.dart';
import 'join_game_request.dart';
import 'join_game_response.dart';

final studentSessionApiProvider = Provider<StudentSessionApi>((ref) {
  return StudentSessionApi(
    ref.watch(apiClientProvider),
    ref.watch(sessionTokenStorageProvider),
  );
});

class StudentSessionApi {
  StudentSessionApi(this._dio, this._tokenStorage);

  final Dio _dio;
  final SessionTokenStorage _tokenStorage;

  Future<JoinGameResponse> join(JoinGameRequest request) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/student-sessions/join',
        data: request.toJson(),
      );

      final data = response.data;
      if (data == null) {
        throw const ApiException('Join response was empty.');
      }

      final joinResponse = JoinGameResponse.fromJson(data);
      await _tokenStorage.save(joinResponse.sessionToken);
      return joinResponse;
    } on DioException catch (exception) {
      throw mapDioException(exception);
    }
  }
}
