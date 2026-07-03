import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/game_complete/presentation/game_complete_screen.dart';
import '../../features/game_lobby/presentation/game_lobby_screen.dart';
import '../../features/join/presentation/join_screen.dart';
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
        builder: (context, state) => const GameLobbyScreen(),
      ),
      GoRoute(
        path: AppRoutes.gameComplete,
        builder: (context, state) => const GameCompleteScreen(),
      ),
    ],
  );
});
