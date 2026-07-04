import '../../../core/models/student_task_response.dart';

class JoinGameResponse {
  const JoinGameResponse({
    required this.studentSessionId,
    required this.studentName,
    required this.sessionToken,
    required this.currentTask,
  });

  final String studentSessionId;
  final String studentName;
  final String sessionToken;
  final StudentTaskResponse currentTask;

  factory JoinGameResponse.fromJson(Map<String, dynamic> json) {
    return JoinGameResponse(
      studentSessionId: json['studentSessionId'] as String,
      studentName: json['studentName'] as String,
      sessionToken: json['sessionToken'] as String,
      currentTask: StudentTaskResponse.fromJson(
        json['currentTask'] as Map<String, dynamic>,
      ),
    );
  }
}
