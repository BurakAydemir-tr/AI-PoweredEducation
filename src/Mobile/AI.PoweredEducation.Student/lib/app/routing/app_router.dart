import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/game_complete/presentation/game_complete_screen.dart';
import '../../features/game_lobby/presentation/game_lobby_screen.dart';
import '../../features/join/presentation/join_screen.dart';
import '../../features/quiz/presentation/quiz_screen.dart';
import 'app_routes.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: AppRoutes.join,
    routes: [
      GoRoute(
        path: AppRoutes.join,
        builder: (context, state) => const JoinScreen(),
      ),
      GoRoute(
        path: AppRoutes.gameLobby,
        builder: (context, state) {
          final args = state.extra;
          return GameLobbyScreen(
            currentTask: args is GameLobbyScreenArgs ? args.currentTask : null,
          );
        },
      ),
      GoRoute(
        path: AppRoutes.quiz,
        builder: (context, state) {
          final args = state.extra as QuizScreenArgs;
          return QuizScreen(
            task: args.task,
            totalTaskCount: args.totalTaskCount,
          );
        },
      ),
      GoRoute(
        path: AppRoutes.gameComplete,
        builder: (context, state) => const GameCompleteScreen(),
      ),
    ],
  );
});
