import 'learning_task_type.dart';
import 'quiz_answer_option.dart';

class StudentTaskResponse {
  const StudentTaskResponse({
    required this.id,
    required this.taskType,
    required this.order,
    this.attemptCount = 0,
    this.question,
    this.optionA,
    this.optionB,
    this.optionC,
    this.optionD,
    this.instructions,
    this.targetLatitude,
    this.targetLongitude,
    this.timeLimitMinutes,
  });

  final String id;
  final LearningTaskType taskType;
  final int order;
  final int attemptCount;
  final String? question;
  final String? optionA;
  final String? optionB;
  final String? optionC;
  final String? optionD;
  final String? instructions;
  final double? targetLatitude;
  final double? targetLongitude;
  final int? timeLimitMinutes;

  factory StudentTaskResponse.fromJson(Map<String, dynamic> json) {
    return StudentTaskResponse(
      id: json['id'] as String,
      taskType: LearningTaskType.fromJson(json['taskType'] as int),
      order: json['order'] as int,
      attemptCount: json['attemptCount'] as int? ?? 0,
      question: json['question'] as String?,
      optionA: json['optionA'] as String?,
      optionB: json['optionB'] as String?,
      optionC: json['optionC'] as String?,
      optionD: json['optionD'] as String?,
      instructions: json['instructions'] as String?,
      targetLatitude: (json['targetLatitude'] as num?)?.toDouble(),
      targetLongitude: (json['targetLongitude'] as num?)?.toDouble(),
      timeLimitMinutes: json['timeLimitMinutes'] as int?,
    );
  }

  String optionTextFor(QuizAnswerOption option) {
    return switch (option) {
      QuizAnswerOption.optionA => optionA ?? '',
      QuizAnswerOption.optionB => optionB ?? '',
      QuizAnswerOption.optionC => optionC ?? '',
      QuizAnswerOption.optionD => optionD ?? '',
    };
  }
}
