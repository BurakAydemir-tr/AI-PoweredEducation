enum QuizAnswerOption {
  optionA(0, 'A'),
  optionB(1, 'B'),
  optionC(2, 'C'),
  optionD(3, 'D');

  const QuizAnswerOption(this.value, this.label);

  final int value;
  final String label;

  static QuizAnswerOption fromJson(int value) {
    return QuizAnswerOption.values.firstWhere(
      (option) => option.value == value,
      orElse: () => throw ArgumentError('Bilinmeyen cevap seçeneği: $value'),
    );
  }
}
