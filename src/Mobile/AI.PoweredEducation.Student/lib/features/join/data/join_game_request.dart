class JoinGameRequest {
  const JoinGameRequest({
    required this.gameCode,
    required this.studentName,
  });

  final String gameCode;
  final String studentName;

  Map<String, dynamic> toJson() {
    return {
      'gameCode': gameCode,
      'studentName': studentName,
    };
  }
}
