import {
  Box,
  Button,
  Container,
  Stack,
  Typography,
} from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import BarChartIcon from '@mui/icons-material/BarChart'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import PsychologyIcon from '@mui/icons-material/Psychology'
import QrCode2Icon from '@mui/icons-material/QrCode2'
import RocketLaunchIcon from '@mui/icons-material/RocketLaunch'
import SportsEsportsIcon from '@mui/icons-material/SportsEsports'
import type { ReactNode } from 'react'
import { BrandLogo } from '../../components/BrandLogo'

const navItems = ['Özellikler', 'Nasıl Çalışır?', 'Fiyatlandırma', 'Kaynaklar']

export function HomePage() {
  return (
    <Box
      sx={{
        bgcolor: '#f7fbff',
        color: '#071b4d',
        minHeight: '100vh',
        overflow: 'hidden',
      }}
    >
      <Box
        component="header"
        sx={{
          bgcolor: 'rgba(255, 255, 255, 0.96)',
          borderBottom: '1px solid rgba(7, 27, 77, 0.06)',
          boxShadow: '0 12px 42px rgba(7, 27, 77, 0.05)',
        }}
      >
        <Container maxWidth={false} sx={{ maxWidth: 1580, px: { xs: 2, md: 6 } }}>
          <Stack
            direction="row"
            sx={{
              alignItems: 'center',
              gap: { xs: 2, md: 4 },
              justifyContent: 'space-between',
              minHeight: { xs: 92, md: 126 },
            }}
          >
            <Box sx={{ flex: '0 0 auto', minWidth: { md: 300 } }}>
              <BrandLogo compact size="large" />
            </Box>

            <Stack
              component="nav"
              direction="row"
              spacing={{ md: 3, lg: 4 }}
              sx={{
                display: { xs: 'none', md: 'flex' },
                flexGrow: 1,
                justifyContent: 'center',
              }}
            >
              {navItems.map((item) => (
                <Typography
                  key={item}
                  component="a"
                  href="#features"
                  sx={{
                    color: '#071b4d',
                    fontSize: 20,
                    fontWeight: 600,
                    textDecoration: 'none',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {item}
                </Typography>
              ))}
            </Stack>

            <Stack direction="row" spacing={{ xs: 1, sm: 2.5 }}>
              <Button
                component={RouterLink}
                size="large"
                sx={{
                  borderRadius: 999,
                  boxShadow: '0 12px 28px rgba(37, 99, 235, 0.26)',
                  display: { xs: 'none', sm: 'inline-flex' },
                  minWidth: { sm: 124, lg: 142 },
                  px: 3,
                }}
                to="/register"
                variant="contained"
              >
                Başlayın
              </Button>
              <Button
                component={RouterLink}
                size="large"
                sx={{
                  bgcolor: '#ffffff',
                  borderColor: 'rgba(7, 27, 77, 0.14)',
                  borderRadius: 999,
                  color: '#071b4d',
                  minWidth: { xs: 116, sm: 128, lg: 148 },
                  px: 3,
                }}
                to="/login"
                variant="outlined"
              >
                Giriş Yap
              </Button>
            </Stack>
          </Stack>
        </Container>
      </Box>

      <Box
        component="main"
        sx={{
          background:
            'radial-gradient(circle at 82% 44%, rgba(66, 153, 225, 0.16), transparent 34%), linear-gradient(180deg, #ffffff 0%, #f7fbff 68%, #eef7ff 100%)',
        }}
      >
        <Container maxWidth={false} sx={{ maxWidth: 1580, px: { xs: 2, md: 6 } }}>
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            spacing={{ xs: 5, md: 4 }}
            sx={{
              alignItems: 'center',
              minHeight: { xs: 'auto', md: 'calc(100vh - 250px)' },
              pt: { xs: 6, md: 6 },
            }}
          >
            <Box sx={{ flex: '0 1 46%', minWidth: 0, zIndex: 1 }}>
              <Typography
                component="h1"
                sx={{
                  color: '#071b4d',
                  fontSize: { xs: 52, sm: 76, md: 88 },
                  fontWeight: 950,
                  letterSpacing: 0,
                  lineHeight: 1.05,
                  maxWidth: 620,
                }}
              >
                Öğrenme
                <br />
                Bir{' '}
                <Box
                  component="span"
                  sx={{
                    background:
                      'linear-gradient(90deg, #47bb43 0%, #25a7d9 54%, #3769e8 100%)',
                    backgroundClip: 'text',
                    color: 'transparent',
                    WebkitBackgroundClip: 'text',
                  }}
                >
                  Macera!
                </Box>
              </Typography>

              <Typography
                sx={{
                  color: '#27324f',
                  fontSize: { xs: 20, md: 25 },
                  lineHeight: 1.55,
                  maxWidth: 610,
                  mt: 4,
                }}
              >
                Yapay zeka destekli öğrenme oyunları ile eğitimi etkileşimli,
                eğlenceli ve unutulmaz hale getirin.
              </Typography>

              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} sx={{ mt: 5 }}>
                <Button
                  component={RouterLink}
                  size="large"
                  startIcon={<RocketLaunchIcon />}
                  sx={{
                    borderRadius: 3,
                    boxShadow: '0 18px 34px rgba(37, 99, 235, 0.24)',
                    fontSize: 22,
                    minHeight: 66,
                    px: 4,
                  }}
                  to="/register"
                  variant="contained"
                >
                  Hemen Başla
                </Button>
                <Button
                  component="a"
                  href="#features"
                  size="large"
                  startIcon={<PlayArrowIcon />}
                  sx={{
                    bgcolor: '#ffffff',
                    borderRadius: 3,
                    fontSize: 22,
                    minHeight: 66,
                    px: 4,
                  }}
                  variant="outlined"
                >
                  Tanıtımı İzle
                </Button>
              </Stack>
            </Box>

            <Box
              sx={{
                flex: '1 1 54%',
                minWidth: 0,
                width: '100%',
              }}
            >
              <Box
                component="img"
                alt="Oyunlaştırılmış öğrenme adası"
                src="/assets/home-adventure-generated-cropped.png"
                sx={{
                  borderRadius: { xs: 4, md: 6 },
                  display: 'block',
                  filter: 'drop-shadow(0 34px 42px rgba(7, 27, 77, 0.16))',
                  height: 'auto',
                  ml: { xs: 'auto', md: -4, lg: -2 },
                  mr: { xs: 'auto', md: -2 },
                  maxHeight: { xs: 430, sm: 540, md: 560, lg: 640 },
                  maxWidth: '100%',
                  objectFit: 'contain',
                  width: { xs: '100%', md: '108%', lg: '106%' },
                }}
              />
            </Box>
          </Stack>

          <Box
            id="features"
            sx={{
              display: 'grid',
              gap: { xs: 2, md: 3 },
              gridTemplateColumns: {
                xs: '1fr',
                sm: 'repeat(2, minmax(0, 1fr))',
                md: 'repeat(4, minmax(0, 1fr))',
              },
              pb: { xs: 5, md: 6 },
              pt: { xs: 4, md: 3 },
            }}
          >
            <FeaturePill
              color="#2f7df1"
              icon={<SportsEsportsIcon />}
              label="Oyunlaştırılmış Öğrenme"
            />
            <FeaturePill color="#52c557" icon={<QrCode2Icon />} label="QR & GPS Görevleri" />
            <FeaturePill
              color="#f28a2d"
              icon={<PsychologyIcon />}
              label="Yapay Zeka ile İlk Taslaklar"
            />
            <FeaturePill color="#9357e8" icon={<BarChartIcon />} label="Detaylı Raporlama" />
          </Box>
        </Container>
      </Box>
    </Box>
  )
}

function FeaturePill({
  color,
  icon,
  label,
}: {
  color: string
  icon: ReactNode
  label: string
}) {
  return (
    <Stack
      direction="row"
      spacing={1.6}
      sx={{
        alignItems: 'center',
        bgcolor: 'rgba(255, 255, 255, 0.72)',
        border: '1px solid rgba(7, 27, 77, 0.06)',
        borderRadius: 2,
        minHeight: 88,
        minWidth: 0,
        px: 2,
        py: 1.5,
      }}
    >
      <Box
        sx={{
          alignItems: 'center',
          bgcolor: `${color}1f`,
          borderRadius: 2,
          color,
          display: 'flex',
          flex: '0 0 auto',
          height: 56,
          justifyContent: 'center',
          width: 56,
          '& svg': { fontSize: 32 },
        }}
      >
        {icon}
      </Box>
      <Typography
        sx={{
          color: '#071b4d',
          fontSize: { xs: 17, md: 18 },
          fontWeight: 650,
          lineHeight: 1.2,
          minWidth: 0,
        }}
      >
        {label}
      </Typography>
    </Stack>
  )
}
