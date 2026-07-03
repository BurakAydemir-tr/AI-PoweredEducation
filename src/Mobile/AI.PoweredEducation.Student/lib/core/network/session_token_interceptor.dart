import 'package:dio/dio.dart';

import '../storage/session_token_storage.dart';

class SessionTokenInterceptor extends Interceptor {
  SessionTokenInterceptor(this._storage);

  static const sessionTokenHeader = 'X-Session-Token';

  final SessionTokenStorage _storage;

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _storage.read();

    if (token != null && token.isNotEmpty) {
      options.headers[sessionTokenHeader] = token;
    }

    handler.next(options);
  }
}
