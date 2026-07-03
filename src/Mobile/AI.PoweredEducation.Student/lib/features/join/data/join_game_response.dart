class JoinGameResponse {
  const JoinGameResponse({
    required this.studentSessionId,
    required this.studentName,
    required this.sessionToken,
  });

  final String studentSessionId;
  final String studentName;
  final String sessionToken;

  factory JoinGameResponse.fromJson(Map<String, dynamic> json) {
    return JoinGameResponse(
      studentSessionId: json['studentSessionId'] as String,
      studentName: json['studentName'] as String,
      sessionToken: json['sessionToken'] as String,
    );
  }
}
