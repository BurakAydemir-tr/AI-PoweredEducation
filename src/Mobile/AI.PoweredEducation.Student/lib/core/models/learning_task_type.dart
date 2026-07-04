enum LearningTaskType {
  quiz(0),
  qrCode(1),
  gps(2);

  const LearningTaskType(this.value);

  final int value;

  String get displayName {
    return switch (this) {
      LearningTaskType.quiz => 'Quiz',
      LearningTaskType.qrCode => 'QR Kod',
      LearningTaskType.gps => 'GPS',
    };
  }

  static LearningTaskType fromJson(int value) {
    return LearningTaskType.values.firstWhere(
      (type) => type.value == value,
      orElse: () => throw ArgumentError('Bilinmeyen görev türü: $value'),
    );
  }
}
