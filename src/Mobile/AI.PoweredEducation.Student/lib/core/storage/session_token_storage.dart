import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

final sessionTokenStorageProvider = Provider<SessionTokenStorage>((ref) {
  return const SessionTokenStorage();
});

class SessionTokenStorage {
  const SessionTokenStorage();

  static const _sessionTokenKey = 'student_session_token';
  static const _storage = FlutterSecureStorage();

  Future<void> save(String token) {
    return _storage.write(key: _sessionTokenKey, value: token);
  }

  Future<String?> read() {
    return _storage.read(key: _sessionTokenKey);
  }

  Future<void> clear() {
    return _storage.delete(key: _sessionTokenKey);
  }
}
