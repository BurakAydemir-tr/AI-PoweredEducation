import { Box, Stack, Typography } from '@mui/material'

type BrandLogoProps = {
  compact?: boolean
  inverted?: boolean
  size?: 'regular' | 'large'
}

export function BrandLogo({
  compact = false,
  inverted = false,
  size = 'regular',
}: BrandLogoProps) {
  const textColor = inverted ? '#ffffff' : '#071b4d'
  const compactImageSize = size === 'large' ? { height: 68, width: 74 } : { height: 54, width: 58 }
  const compactFontSize = size === 'large' ? { xs: 23, sm: 28 } : { xs: 19, sm: 25 }

  return (
    <Stack
      direction="row"
      spacing={compact ? 1.2 : 1.8}
      sx={{ alignItems: 'center', minWidth: 0 }}
    >
      <Box
        component="img"
        alt="AI Powered Education"
        src="/assets/brand-logo-glow.png"
        sx={{
          display: 'block',
          flex: '0 0 auto',
          height: compact ? compactImageSize.height : { xs: 70, md: 92 },
          objectFit: 'contain',
          width: compact ? compactImageSize.width : { xs: 78, md: 104 },
        }}
      />
      <Typography
        component="div"
        sx={{
          color: textColor,
          fontSize: compact ? compactFontSize : { xs: 32, sm: 40 },
          fontWeight: 900,
          letterSpacing: 0,
          lineHeight: 0.92,
          whiteSpace: 'nowrap',
        }}
      >
        <Box component="span" sx={{ color: '#43b955' }}>
          AI
        </Box>{' '}
        Powered
        <br />
        Education
      </Typography>
    </Stack>
  )
}
